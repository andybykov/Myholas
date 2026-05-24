using Myholas.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntityDto?> GetByIdAsync(int id);
        Task<UserEntityDto?> GetByUsernameAsync(string username);
        Task<UserEntityDto?> GetByEmailAsync(string email);
        Task<UserEntityDto> CreateAsync(UserEntityDto user, string plainPassword); // внутри хеширует
        Task<bool> ValidatePasswordAsync(string username, string plainPassword);
        Task<bool> UpdatePasswordAsync(int userId, string newPlainPassword);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> DeleteAsync(int userId);
        Task<bool> IsAdminAsync(int userId);
        Task<RefreshTokenEntity> CreateRefreshTokenAsync(int userId, int expiryDays = 7);
        Task<RefreshTokenEntity?> GetRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token);
    }
}
