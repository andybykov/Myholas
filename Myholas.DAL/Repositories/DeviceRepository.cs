using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Myholas.Core;
using Myholas.Core.Dtos;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Interfaces;
using Npgsql;
using System.Diagnostics;

namespace Myholas.DAL.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DeviceRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // Получить сушность / EntityDto
        public async Task<EntityDto?> GetByEntityIdAsync(string entityId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            return await context.Entities.FirstOrDefaultAsync(e => e.EntityId == entityId);
        }

        // Получить все физические устройства вместе с их датчиками
        public async Task<List<DeviceDto>> GetAllDevicesAsync(bool includeUnavailable = false)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var query = context.Devices.Include(d => d.Entities).AsQueryable();

           
            // фильтруем по физическому устройству
            if (!includeUnavailable)
                query = query.Where(d => d.IsOnline == true);

            return await query.OrderBy(d => d.FriendlyName).ToListAsync();
        }

        // Получить все сущности по домену 
        public async Task<List<EntityDto>> GetByDomainAsync(string domain)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            return await context.Entities
                .Where(e => e.Domain == domain)
                .ToListAsync();
        }

        // Получить физическое устройство по его DeviceID 
        public async Task<DeviceDto?> GetByDeviceIdAsync(string deviceId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            return await context.Devices
                .Include(d => d.Entities)
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId);
        }

        // Создает устройство и сущность
        public async Task<EntityDto> AddOrUpdateEntityAsync(DeviceDto deviceDto, EntityDto entityDto)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            while (true)
            {
                try
                {  
                    // УСТРОЙСТВО
                    var device = await context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceDto.DeviceId); // тот же ID
                    if (device == null)
                    {
                        device = deviceDto;
                        await context.Devices.AddAsync(device);
                        
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        // НЕ NULL поля
                        if (!string.IsNullOrEmpty(deviceDto.FriendlyName)) device.FriendlyName = deviceDto.FriendlyName;
                        if (!string.IsNullOrEmpty(deviceDto.IpAddress)) device.IpAddress = deviceDto.IpAddress;
                        if (!string.IsNullOrEmpty(deviceDto.Version)) device.Version = deviceDto.Version;
                      

                        await context.SaveChangesAsync();
                    }

                    // СУЩНОСТЬ
                    var entity = await context.Entities.FirstOrDefaultAsync(e => e.EntityId == entityDto.EntityId);
                    if (entity == null)
                    {
                        entityDto.DeviceId = device.Id; 
                        entity = entityDto;

                        await context.Entities.AddAsync(entity);
                    }
                    else
                    {
                        // только заполненные поля
                        if (!string.IsNullOrEmpty(entityDto.FriendlyName)) entity.FriendlyName = entityDto.FriendlyName;
                        if (!string.IsNullOrEmpty(entityDto.Domain)) entity.Domain = entityDto.Domain;
                        if (!string.IsNullOrEmpty(entityDto.StateTopic)) entity.StateTopic = entityDto.StateTopic;
                        if (!string.IsNullOrEmpty(entityDto.CommandTopic)) entity.CommandTopic = entityDto.CommandTopic;
                        if (!string.IsNullOrEmpty(entityDto.UnitOfMeasurement)) entity.UnitOfMeasurement = entityDto.UnitOfMeasurement;
                        if (!string.IsNullOrEmpty(entityDto.AttributesJson)) entity.AttributesJson = entityDto.AttributesJson;

                        entity.DeviceId = device.Id;
                    }

                    await context.SaveChangesAsync();

                  
                    return entity;
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    // При конфликте ключей очищаем трекер и пробуем снова для избежания "23505"
                    Console.WriteLine($"[DeviceRepo] Conflict detected for {entityDto.EntityId}, retrying...");
                    context.ChangeTracker.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DeviceRepo] Unexpected error: {ex.Message}");
                    throw;
                }
            }           
        }


        // Обновить online/offline
        public async Task<bool> UpdateDeviceStatusAsync(DeviceDto deviceDto)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    
           
            var device = await context.Devices
                .FirstOrDefaultAsync(d => d.DeviceId == deviceDto.DeviceId);


            if (device == null)
            {
                return false;
            }

            //  Обновляем 
            device.IsOnline = deviceDto.IsOnline;
            device.LastSeen = DateTime.UtcNow; 


            await context.SaveChangesAsync();

            return true;
        }


        // Удаление cущности
        public async Task<bool> DeleteEntityAsync(string entityId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            var entity = await context.Entities.FirstOrDefaultAsync(e => e.EntityId == entityId);
            if (entity == null)
                return false;

            context.Entities.Remove(entity);
            await context.SaveChangesAsync();

            return true;
        }

        // Удаление cущности
        public async Task<bool> DeleteDeviceAsync(string deviceId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var device = await context.Devices.FirstOrDefaultAsync(e => e.DeviceId == deviceId);
            if (device == null)
                return false;

            context.Devices.Remove(device);
            await context.SaveChangesAsync();

            return true;
        }

        // существует ли
        public async Task<bool> ExistsAsync(string entityId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            return await context.Entities.AnyAsync(e => e.EntityId == entityId);
        }
    }
}
