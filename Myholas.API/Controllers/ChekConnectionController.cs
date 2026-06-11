using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Myholas.Core.Models.Input;

namespace Myholas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChekConnectionController : ControllerBase
    {
        // Открытый endpoint
        [AllowAnonymous]
        [HttpGet()]
        public async Task<IActionResult> Chek()
        {
            return Ok();
        }
    }
}
