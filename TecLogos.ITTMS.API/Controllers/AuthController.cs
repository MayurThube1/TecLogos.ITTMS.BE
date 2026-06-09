using Microsoft.AspNetCore.Mvc;

namespace TecLogos.ITTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping() => Ok("Auth controller is alive");
    }
}
