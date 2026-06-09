using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Input;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Myholas.API.Controllers
{
    // API‑контроллер аутентификации
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserManager _userManager;
        // Супер секретный ключ для подписи JWT 
        private readonly string _jwtKey = "mysupersecretkey_32bytes_long!!!!!";

        public AuthController(IUserManager userManager)
        {
            _userManager = userManager; 
        }

        // Открытый endpoint
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginInputModel login)
        {

            bool validUser = await _userManager.ValidatePasswordAsync(
                login.Username,
                login.Password);

            if (!validUser)
             
                return Unauthorized("Incorrect login or password");

            
            var user = await _userManager.GetByUsernameAsync(login.Username);

            if (user !=null && user.IsActive == false)
               return Forbid();

            if (user != null)
                await _userManager.UpdateLastLoginAsync(user.Id);

            // Claim
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username), // имя пользователя
                new Claim(ClaimTypes.Role, user.Role.ToString()), // роль 
                new Claim("userId", user.Id.ToString()) // собственный идентификатор
            };

            // Ключ и подпись токена
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // JWT
            var token = new JwtSecurityToken(
                issuer: "MyholasServer",
                audience: "MyholasClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),   // токен действителен 
                signingCredentials: creds);

            // Возвращаем клиенту готовый токен в виде строки
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
