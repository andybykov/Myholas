using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Myholas.Core.Enums;

namespace Myholas.Core.Interfaces
{
    public interface IUserManager
    {
        Task<UserEntityOutputModel?> GetByIdAsync(int id);


        Task<UserEntityOutputModel?> GetByUsernameAsync(string username);


        Task<UserEntityOutputModel> CreateAsync(UserEntityInputModel user, string password);


        Task<bool> ValidatePasswordAsync(string username, string plainPassword);


        Task<bool> UpdatePasswordAsync(int userId, string newPassword);


        Task<bool> UpdateLastLoginAsync(int userId);

        Task<bool> SetRoleUser(int userId);

        Task<bool> SetRoleAdmin(int userId);


        Task<bool> DeleteAsync(int userId);


        Task<bool> IsAdminAsync(int userId);


        Task<List<UserEntityOutputModel>> GetByRoleAsync(UserRole role);
    }
}
