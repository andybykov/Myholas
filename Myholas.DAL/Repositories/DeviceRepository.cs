using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Myholas.Core;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;
using Npgsql;


namespace Myholas.DAL.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DeviceRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        //Получить устройство по EntityId
        public async Task<DeviceEntityDto?> GetByIdAsync(string entityId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                return await context.Devices.FirstOrDefaultAsync(d => d.EntityId == entityId);
            }
        }

        //Получить все устройства
        public async Task<List<DeviceEntityDto>> GetAllAsync(bool includeUnavailable = false)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var query = context.Devices.AsQueryable();
                if (!includeUnavailable)
                    query = query.Where(d => d.IsAvailable);

                return await query.OrderBy(d => d.FriendlyName).ToListAsync();
            }
        }

        //Получить устройства по домену (switch, sensor, light, select)
        public async Task<List<DeviceEntityDto>> GetByDomainAsync(string domain)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                return await context.Devices.Where(d => d.Domain == domain).ToListAsync();
            }
        }

        //Получить устройства по физическому ID
        public async Task<List<DeviceEntityDto>> GetByDeviceIdAsync(string deviceId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                return await context.Devices.Where(d => d.DeviceId == deviceId).ToListAsync();
            }
        }


        // Добавить новое устройство или обновить существующее

        // МЕТОД пытается бороться RACE CONDITION
        //  при параллельных вызовах для одного EntityId 
        // оба потока могут пройти проверку AnyAsync() и попытаться вставить новую запись....
        // получаем исключени: "PG 23505" нарушение PK_Devices!

        // НО навреное лучше использовать даппер....

        public async Task<DeviceEntityDto> AddOrUpdateAsync(DeviceEntityDto entity)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                DeviceEntityDto? existing = null;
                bool saved = false;

                while (!saved)
                {
                    // Загружаем существующую запись
                    existing = await context.Devices
                        .AsTracking()
                        .FirstOrDefaultAsync(d => d.EntityId == entity.EntityId);

                    if (existing != null)
                    {
                        // копируем все поля из entity
                        context.Entry(existing).CurrentValues.SetValues(entity);
                        existing.UpdatedAt = DateTime.UtcNow; // CreatedAt исходное

                    }
                    else
                    {
                        // Новая запись
                        entity.CreatedAt = DateTime.UtcNow;
                        await context.Devices.AddAsync(entity);
                    }

                    try
                    {
                        await context.SaveChangesAsync();
                        saved = true; // сохранено
                    }
                    // 23505 = duplicate key value violates unique constraint
                    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                    {
                        Console.WriteLine($"[AddOrUpdate] Conflict for {entity.EntityId}, retrying...");
                        context.ChangeTracker.Clear();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AddOrUpdate] Unexpected error: {ex.Message}");
                        throw;
                    }
                }

                return existing ?? entity;
            }
        }

        //Удалить устройство по EntityId
        public async Task<bool> DeleteAsync(string entityId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var device = await context.Devices.FirstOrDefaultAsync(d => d.EntityId == entityId);
                if (device == null) return false;
                context.Devices.Remove(device);
                await context.SaveChangesAsync();
                return true;
            }
        }

        //Проверить существование устройства
        public async Task<bool> ExistsAsync(string entityId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                return await context.Devices.AnyAsync(d => d.EntityId == entityId);
            }
        }

        // STATES!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! in state repo!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
      
        //Обновить текущее состояние устройства и добавить запись в историю
        public async Task UpdateStateAsync(StateEntityDto stateEntity)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var device = await context.Devices.FirstOrDefaultAsync(d => d.EntityId == stateEntity.EntityId);
                if (device != null)
                {
                    device.CurrentState = stateEntity.State;
                    device.LastSeen = DateTime.UtcNow;
                    device.UpdatedAt = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(stateEntity.AttributesJson))
                        device.AttributesJson = stateEntity.AttributesJson;

                    var historyEntry = new StateEntityDto
                    {
                        EntityId = stateEntity.EntityId,
                        State = stateEntity.State,
                        AttributesJson = stateEntity.AttributesJson,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.States.AddAsync(historyEntry);
                    await context.SaveChangesAsync();
                }
            }
        }

        //Получить историю состояний устройства
        public async Task<List<StateEntityDto>> GetStatesByEntityIdAsync(string entityId, int limit = 100)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                return await context.States
                    .Where(s => s.EntityId == entityId)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(limit)
                    .ToListAsync();
            }
        }

        //Добавить запись состояния
        public async Task AddStateAsync(StateEntityDto stateEntity)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                stateEntity.CreatedAt = DateTime.UtcNow;
                await context.States.AddAsync(stateEntity);
                await context.SaveChangesAsync();
            }
        }

        //Обновить статус доступности устройства
        public async Task UpdateAvailabilityAsync(DeviceEntityDto deviceDto)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var device = await context.Devices.FirstOrDefaultAsync(d => d.EntityId == deviceDto.EntityId);
                if (device != null)
                {
                    device.IsAvailable = deviceDto.IsAvailable;
                    device.UpdatedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}