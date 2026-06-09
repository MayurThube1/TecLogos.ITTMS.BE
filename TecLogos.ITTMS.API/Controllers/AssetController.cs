using Microsoft.AspNetCore.Mvc;

namespace TecLogos.ITTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping() => Ok("Asset controller is alive");
    }
}
