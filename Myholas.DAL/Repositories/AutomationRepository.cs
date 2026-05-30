using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Myholas.Core;
using Myholas.Core.Dtos.Automations;
using Myholas.Core.Interfaces;

namespace Myholas.DAL.Repositories
{
    public class AutomationRepository : IAutomationRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AutomationRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<AutomationEntityDto?> GetByIdAsync(int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            return await ctx.Automations.FindAsync(id);
        }

        public async Task<List<AutomationEntityDto>> GetAllAsync(bool includeDisabled = false)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
            var query = ctx.Automations.AsQueryable();
            if (!includeDisabled)
                query = query.Where(a => a.IsEnabled);

            return await query.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<List<AutomationEntityDto>> GetEnabledAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            return await ctx.Automations.Where(a => a.IsEnabled).ToListAsync();
        }

        public async Task<AutomationEntityDto> AddAsync(AutomationEntityDto automation)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
            automation.CreatedAt = DateTime.UtcNow;
            await ctx.Automations.AddAsync(automation);
            await ctx.SaveChangesAsync();

            return automation;
        }

        public async Task<AutomationEntityDto> UpdateAsync(AutomationEntityDto automation)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
            automation.UpdatedAt = DateTime.UtcNow;
            ctx.Automations.Update(automation);
            await ctx.SaveChangesAsync();

            return automation;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
            var a = await ctx.Automations.FindAsync(id);
            if (a == null) return false;
            ctx.Automations.Remove(a);
            await ctx.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetEnabledAsync(int id, bool enabled)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
            var a = await ctx.Automations.FindAsync(id);
            if (a == null) return false;
            a.IsEnabled = enabled;
            a.UpdatedAt = DateTime.UtcNow;
            await ctx.SaveChangesAsync();

            return true;
        }

    }
}
