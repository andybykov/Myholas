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
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly string _jwtKey = "mysupersecretkey_32bytes_long!!!!!";

        public AuthController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        [AllowAnonymous] // доступен всем
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginInputModel login)
        {            
            bool validUser = false;

            validUser =  await _userManager.ValidatePasswordAsync(login.Username, login.Password);

            if (!validUser)
                return Unauthorized("Incorrect login or password");

            var user = await _userManager.GetByUsernameAsync(login.Username);                
            // Claims == права пользователя
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()), 
            new Claim("userId", user.Id.ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "MyholasServer",
                audience: "MyholasClient",
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }   
}
