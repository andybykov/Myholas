using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;
using static Myholas.Core.Enums;

namespace Myholas.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManager _userManager;

        public UsersController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        // GET api/users/{id}
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserEntityOutputModel>> GetById(int id)
        {
            var user = await _userManager.GetByIdAsync(id);
            if (user == null)
                return NotFound($"User with id {id} not found");

            return Ok(user);
        }

        // GET api/users/by-username/{username}
        [HttpGet("by-username/{username}")]
        public async Task<ActionResult<UserEntityOutputModel>> GetByUsername(string username)
        {
            var user = await _userManager.GetByUsernameAsync(username);
            if (user == null)
                return NotFound($"User with username '{username}' not found");

            return Ok(user);
        }

        // POST api/users Create user
        [HttpPost]
        public async Task<ActionResult<UserEntityOutputModel>> Create([FromBody] UserEntityInputModel user)
        {
            if (user == null)
                return BadRequest("User data is required");

            if (string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Password is required");

            var notAvaibleName = await _userManager.GetByUsernameAsync(user.UserName);

            if (notAvaibleName is not null && notAvaibleName.Username is not null)
                return BadRequest($"Name {user.UserName} is not avaible");

            var createdUser = await _userManager.CreateAsync(user, user.Password);


            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        // POST api/users/validate-password
        [HttpPost("validate-password")]
        public async Task<ActionResult<bool>> ValidatePassword([FromBody] UserLoginInputModel user)
        {


            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Username and password are required");

            var isValid = await _userManager.ValidatePasswordAsync(user.Username, user.Password);
            return Ok(isValid);
        }

        // PUT api/users/update/password        
        // Обновить пароль 
        [Authorize]
        [HttpPut("update/password")]
        public async Task<ActionResult<bool>> UpdatePassword([FromBody] UserLoginInputModel user)
        {
            var validUser = await _userManager.GetByUsernameAsync(user.Username);
            var newPass = user.Password;
            if (validUser is null)
                return BadRequest("Not valid user");

            if (string.IsNullOrWhiteSpace(newPass))
                return BadRequest("New password is required");

            var result = await _userManager.UpdatePasswordAsync(validUser.Id, newPass);
            if (!result)
                return StatusCode(500, "Failed to update password");

            return Ok(true);
        }


        // PUT api/users/update/last-login
        [Authorize]
        [HttpPut("update/last-login")]
        public async Task<ActionResult<bool>> UpdateLastLogin(int userId)
        {
            var result = await _userManager.UpdateLastLoginAsync(userId);
            if (!result)
                return StatusCode(500, "Failed to update last login");

            return Ok(true);
        }

        // DELETE api/users/{userId}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> Delete(int userId)
        {
            var result = await _userManager.DeleteAsync(userId);
            if (!result)
                return StatusCode(500, "Failed to delete user");

            return NoContent();
        }

        // GET api/users/{userId}/is-admin
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId:int}/is-admin")]
        public async Task<ActionResult<bool>> IsAdmin(int userId)
        {
            var isAdmin = await _userManager.IsAdminAsync(userId);
            return Ok(isAdmin);
        }

        // GET api/users/{userId}/set-user
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId:int}/set-user")]
        public async Task<ActionResult<bool>> SetRoleUser(int userId)
        {
            var res = await _userManager.SetRoleUser(userId);
            return Ok(res);
        }


        // GET api/users/{userId}/set-user
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId:int}/set-admin")]
        public async Task<ActionResult<bool>> SetRoleAdmin(int userId)
        {
            var res = await _userManager.SetRoleAdmin(userId);
            return Ok(res);
        }

        // GET api/users/by-role/{role}
        [Authorize(Roles = "Admin")]
        [HttpGet("by-role/{role}")]
        public async Task<ActionResult<List<UserEntityOutputModel>>> GetByRole(UserRole role)
        {
            var users = await _userManager.GetByRoleAsync(role);
            return Ok(users);
        }


    }
}