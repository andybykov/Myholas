using AutoMapper;
using Myholas.Core.Dtos.Users;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;
using static Myholas.Core.Enums;

namespace Myholas.BLL.User
{
    public class UserManager : IUserManager
    {
        private readonly IEventBus _eventBus;

        private readonly IUserRepository _userRep;

        private readonly IMapper _mapper;

        public UserManager(IEventBus eventBus, IUserRepository userRep, IMapper mapper)
        {
            _eventBus = eventBus;
            _userRep = userRep;
            _mapper = mapper;
        }

        public async Task<UserEntityOutputModel?> GetByIdAsync(int id)
        {
            var user = await _userRep.GetByIdAsync(id);

            return _mapper.Map<UserEntityOutputModel>(user);
        }


        public async Task<UserEntityOutputModel?> GetByUsernameAsync(string username)
        {
            var user = await _userRep.GetByUsernameAsync(username);

            return _mapper.Map<UserEntityOutputModel>(user);
        }


        public async Task<UserEntityOutputModel> CreateAsync(UserEntityInputModel user, string password)
        {
            //  существует ли пользователь
            var existingUser = await _userRep.GetByUsernameAsync(user.UserName);
            if (existingUser != null)
                throw new InvalidOperationException($"User with username '{user.UserName}' already exists");

            var dto = _mapper.Map<UserEntityDto>(user);
            var newUserDto = await _userRep.CreateAsync(dto, password);

            return _mapper.Map<UserEntityOutputModel>(newUserDto);
        }

        // сравнивает введенный пароль с сохраненным хешем
        public async Task<bool> ValidatePasswordAsync(string username, string plainPassword)
        {
            return await _userRep.ValidatePasswordAsync(username, plainPassword);
        }


        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            return await _userRep.UpdatePasswordAsync(userId, newPassword);
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            return await _userRep.UpdateLastLoginAsync(userId);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            return await _userRep.DeleteAsync(userId);
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            return await _userRep.IsAdminAsync(userId);
        }

        public async Task<bool> SetRoleUser(int userId)
        {
            return await _userRep.SetRoleUser(userId);
        }

        public async Task<bool> SetRoleAdmin(int userId)
        {
            return await _userRep.SetRoleAdmin(userId);
        }

        public async Task<List<UserEntityOutputModel>> GetByRoleAsync(UserRole role)
        {
            var users = await _userRep.GetByRoleAsync(role);

            List<UserEntityOutputModel> usersOut = new();
            foreach (var user in users)
            {
                var addUser = _mapper.Map<UserEntityOutputModel>(user);
                usersOut.Add(addUser);
            }

            return usersOut;

        }
    }
}