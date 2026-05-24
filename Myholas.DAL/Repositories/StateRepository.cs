using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Myholas.Core;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;

namespace Myholas.DAL.Repositories
{
    // Репозиторий для работы с историей состояний устройств
    public class StateRepository : IStateRepository
    {
        // изолированные контексты БД
        private readonly IServiceScopeFactory _scopeFactory;

        public StateRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }


        // Добавить запись в ИСТОРИЮ состояний
        //DTO состояния (EntityId, State, AttributesJson)
        public async Task<StateEntityDto> AddStateAsync(StateEntityDto state)
        {

            state.CreatedAt = DateTime.UtcNow;


            using (var scope = _scopeFactory.CreateScope())
            {
                //DbContext из DI
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // существует ли устройство с EntityId
                var deviceExists = await context.Devices.AnyAsync(d => d.EntityId == state.EntityId);

                if (!deviceExists)
                {
                    // FK_States_Devices_EntityId
                    var stubDevice = new DeviceEntityDto
                    {
                        EntityId = state.EntityId,
                        //  deviceId из entityId 
                        DeviceId = state.EntityId.Split('.').LastOrDefault() ?? "unknown",
                        //  домен из entityId
                        Domain = state.EntityId.Split('.').FirstOrDefault() ?? "unknown",

                        FriendlyName = state.EntityId, // Временно
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.Devices.AddAsync(stubDevice);
                    await context.SaveChangesAsync();
                }

                // запись состояния
                await context.States.AddAsync(state);
                await context.SaveChangesAsync();
            }

            return state;
        }

        // Получает историю состояний устройства с фильтрацией по дате        
        public async Task<List<StateEntityDto>> GetHistoryAsync(string entityId, DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                //  все состояния
                var query = context.States.Where(s => s.EntityId == entityId);

                //  фильтры
                if (from.HasValue)
                    query = query.Where(s => s.CreatedAt >= from.Value);
                if (to.HasValue)
                    query = query.Where(s => s.CreatedAt <= to.Value);

                // Сортируем 
                return await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(limit)
                    .ToListAsync();
            }
        }


        // Получает последнее состояние
        public async Task<StateEntityDto?> GetLastStateAsync(string entityId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                return await context.States
                    .Where(s => s.EntityId == entityId)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();
            }
        }


        // Удаляет все состояния больше указанной даты        
        public async Task DeleteOldAsync(DateTime olderThan)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // все старые записи
                var old = context.States.Where(s => s.CreatedAt < olderThan);

                // Удаляем 
                context.States.RemoveRange(old);
                await context.SaveChangesAsync();
            }
        }
    }
}