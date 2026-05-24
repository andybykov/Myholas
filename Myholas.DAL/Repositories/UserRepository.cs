using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Myholas.Core;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;
using BCrypt.Net;

namespace Myholas.DAL.Repositories
{

    /// Репозиторий для работы с пользователями и аутентификацией
    public class UserRepository : IUserRepository
    {
        // Предотвращает (нет!) ошибки параллельного доступа к DbContext
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


       
        public async Task<UserEntityDto?> GetByEmailAsync(string email)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
        }

       
        public async Task<UserEntityDto> CreateAsync(UserEntityDto user, string plainPassword)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // Хешируем пароль
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
                user.CreatedAt = DateTime.UtcNow;

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                return user;
            }
        }

        
        public async Task<bool> ValidatePasswordAsync(string username, string plainPassword)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null) return false;

            // BCrypt.Verify сравнивает введённый пароль с сохранённым хешем
            return BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash);
        }

       
        public async Task<bool> UpdatePasswordAsync(int userId, string newPlainPassword)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var user = await context.Users.FindAsync(userId);
                if (user == null) return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPlainPassword);
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
                if (user == null) return false;

                user.LastLogin = DateTime.UtcNow;
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
                if (user == null) return false;

                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            return user != null && user.Role == "admin";
        }

       
        public async Task<RefreshTokenEntity> CreateRefreshTokenAsync(int userId, int expiryDays = 7)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var token = new RefreshTokenEntity
                {
                    // Генерируем  GUID
                    Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                    ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                await context.RefreshTokens.AddAsync(token);
                await context.SaveChangesAsync();
                return token;
            }
        }

      
        public async Task<RefreshTokenEntity?> GetRefreshTokenAsync(string token)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                return await context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == token);
            }
        }

       
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var rt = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
                if (rt == null) return false;

                context.RefreshTokens.Remove(rt);
                await context.SaveChangesAsync();
                return true;
            }
        }
    }
}