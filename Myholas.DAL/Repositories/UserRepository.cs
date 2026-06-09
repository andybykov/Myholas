using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Myholas.Core;
using Myholas.Core.Dtos.Users;
using Myholas.Core.Interfaces;
using static Myholas.Core.Enums;

namespace Myholas.DAL.Repositories
{

    /// Репозиторий для работы с пользователями и аутентификацией
    public class UserRepository : IUserRepository
    {
        // Предотвращает ошибки параллельного доступа к DbContext
        private readonly IServiceScopeFactory _scopeFactory;

        public UserRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }


        // Получить пользователя по ID
        public async Task<UserEntityDto?> GetByIdAsync(int id)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                return await context.Users.FindAsync(id);
            }
        }


        public async Task<UserEntityDto?> GetByUsernameAsync(string username)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
            }
        }


        public async Task<UserEntityDto> CreateAsync(UserEntityDto user, string password)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // Хешируем пароль
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.CreatedAt = DateTime.UtcNow;

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                return user;
            }
        }

        public async Task<bool> ValidatePasswordAsync(string username, string plainPassword)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null) 
                return false;

            // BCrypt.Verify сравнивает введенный пароль с сохраненным хешем
            return BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash);
        }


        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var user = await context.Users.FindAsync(userId);

                if (user == null)
                    return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await context.SaveChangesAsync();

                return true;
            }
        }


        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var user = await context.Users.FindAsync(userId);

                if (user == null)
                    return false;

                user.LastLogin = DateTime.UtcNow;
                await context.SaveChangesAsync();

                return true;
            }
        }

        public async Task<bool> SetRoleUser(int userId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var user = await context.Users.FindAsync(userId);

                if (user == null)
                    return false;

                user.Role = UserRole.User;
                await context.SaveChangesAsync();

                return true;
            }
        }

        public async Task<bool> SetRoleAdmin(int userId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var user = await context.Users.FindAsync(userId);

                if (user == null)
                    return false;

                user.Role = UserRole.Admin;
                await context.SaveChangesAsync();

                return true;
            }
        }


        public async Task<bool> DeleteAsync(int userId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var user = await context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                context.Users.Remove(user);
                await context.SaveChangesAsync();

                return true;
            }
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            var user = await GetByIdAsync(userId);

            return user != null && UserRole.Admin == user.Role;
        }


        public async Task<List<UserEntityDto>> GetByRoleAsync(UserRole role)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var users = await context.Users.Where(u => UserRole.Admin == role)  // фильтрация по роли
                                            .ToListAsync();
                if (users == null)
                    return new();

                return users;
            }
        }
    }
}