using Myholas.Core.Dtos.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Myholas.Core.Enums;

namespace Myholas.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntityDto?> GetByIdAsync(int id);

        Task<UserEntityDto?> GetByUsernameAsync(string username);

        Task<List<UserEntityDto>> GetByRoleAsync(UserRole role);

        Task<UserEntityDto> CreateAsync(UserEntityDto user, string password); // внутри хеширует

        Task<bool> ValidatePasswordAsync(string username, string password);

        Task<bool> UpdatePasswordAsync(int userId, string newPassword);

        Task<bool> UpdateLastLoginAsync(int userId);

        Task<bool> SetRoleUser(int userId);

        Task<bool> SetRoleAdmin(int userId);

        Task<bool> DeleteAsync(int userId);

        Task<bool> IsAdminAsync(int userId); 
    }
}
