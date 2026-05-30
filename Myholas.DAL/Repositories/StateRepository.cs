using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Myholas.Core;
using Myholas.Core.Dtos;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Interfaces;

namespace Myholas.DAL.Repositories
{
    public class StateRepository : IStateRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public StateRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // Обновляет текущее состояние в Entity и добавляет запись в историю
        public async Task UpdateStateAsync(string deviceId, string entityId, string state, string? attributesJson = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var entity = await context.Entities.FirstOrDefaultAsync(e => e.EntityId == entityId);

            if (entity == null)
            {
                // Теперь мы создаем заглушку, используя ПРАВИЛЬНЫЙ deviceId
                entity = await CreateStubEntityAsync(context, deviceId, entityId);
            }

            entity.CurrentState = state;
            entity.UpdatedAt = DateTime.UtcNow;

            var historyEntry = new StateEntityDto
            {
                EntityId = entity.Id,
                State = state,
                AttributesJson = attributesJson,
                CreatedAt = DateTime.UtcNow
            };

            await context.States.AddAsync(historyEntry);
            await context.SaveChangesAsync();
        }

        // Вспомогательный метод для создания заглушки, если пришло состояние неизвестного датчика
        private async Task<EntityDto> CreateStubEntityAsync(DataContext context, string deviceId, string entityId)
        {
            // Ищем, нет ли уже такого устройства (чтобы не плодить дубли)
            var device = await context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);

            if (device == null)
            {
                device = new DeviceDto
                {
                    DeviceId = deviceId, // "esp-lamp01"
                    FriendlyName = "Unknown Device",
                    IsOnline = true
                };
                await context.Devices.AddAsync(device);
                await context.SaveChangesAsync();
            }

            var entity = new EntityDto
            {
                EntityId = entityId,
                Domain = entityId.Split('.').FirstOrDefault() ?? "unknown",
                DeviceId = device.Id, // Привязываем к правильному устройству
                FriendlyName = entityId
            };
            await context.Entities.AddAsync(entity);
            await context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<StateEntityDto>> GetHistoryAsync(string entityId, DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            // Сначала находим внутренний ID сущности по её строковому EntityId
            var entity = await context.Entities.FirstOrDefaultAsync(e => e.EntityId == entityId);
            if (entity == null) return new List<StateEntityDto>();

            var query = context.States.Where(s => s.EntityId == entity.Id);

            if (from.HasValue) query = query.Where(s => s.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(s => s.CreatedAt <= to.Value);

            return await query.OrderByDescending(s => s.CreatedAt).Take(limit).ToListAsync();
        }

        public async Task<StateEntityDto?> GetLastStateAsync(string entityId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var entity = await context.Entities.FirstOrDefaultAsync(e => e.EntityId == entityId);
            if (entity == null) return null;

            return await context.States
                .Where(s => s.EntityId == entity.Id)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteOldAsync(DateTime olderThan)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            var old = context.States.Where(s => s.CreatedAt < olderThan);
            context.States.RemoveRange(old);
            await context.SaveChangesAsync();
        }
    }
}
