using Microsoft.AspNetCore.Mvc;
using Myholas.BLL.User;
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

        // POST api/users
        [HttpPost]
        public async Task<ActionResult<UserEntityOutputModel>> Create([FromBody] UserEntityInputModel user)
        {
            if (user == null)
                return BadRequest("User data is required");

            if (string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Password is required");

            // ВАЖНО: предполагается, что в UserEntityOutputModel есть поле Id (int)
            var createdUser = await _userManager.CreateAsync(user, user.Password);

            // Если Id нет, замените new { id = createdUser.Id } на new { username = createdUser.Username }
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Username }, createdUser);
        }

        // POST api/users/validate-password
        [HttpPost("validate-password")]
        public async Task<ActionResult<bool>> ValidatePassword([FromBody] string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest("Username and password are required");

            var isValid = await _userManager.ValidatePasswordAsync(username, password);
            return Ok(isValid);
        }

        // PUT api/users/{userId}/password
        [HttpPut("{userId:int}/password")]
        public async Task<ActionResult<bool>> UpdatePassword(int userId, [FromBody] string newPass)
        {
            if (userId <= 0)
                return BadRequest("Valid userId is required");

            if (string.IsNullOrWhiteSpace(newPass))
                return BadRequest("New password is required");

            var result = await _userManager.UpdatePasswordAsync(userId, newPass);
            if (!result)
                return StatusCode(500, "Failed to update password");

            return Ok(true);
        }

        // POST api/users/{userId}/last-login
        [HttpPost("{userId:int}/last-login")]
        public async Task<ActionResult<bool>> UpdateLastLogin(int userId)
        {
            var result = await _userManager.UpdateLastLoginAsync(userId);
            if (!result)
                return StatusCode(500, "Failed to update last login");

            return Ok(true);
        }

        // DELETE api/users/{userId}
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> Delete(int userId)
        {
            var result = await _userManager.DeleteAsync(userId);
            if (!result)
                return StatusCode(500, "Failed to delete user");

            return NoContent();
        }

        // GET api/users/{userId}/is-admin
        [HttpGet("{userId:int}/is-admin")]
        public async Task<ActionResult<bool>> IsAdmin(int userId)
        {
            var isAdmin = await _userManager.IsAdminAsync(userId);
            return Ok(isAdmin);
        }

        // GET api/users/by-role/{role}
        [HttpGet("by-role/{role}")]
        public async Task<ActionResult<List<UserEntityOutputModel>>> GetByRole(UserRole role)
        {
            var users = await _userManager.GetByRoleAsync(role);
            return Ok(users);
        }

        
    }
}