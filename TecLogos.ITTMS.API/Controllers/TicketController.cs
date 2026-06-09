using Microsoft.AspNetCore.Mvc;

namespace TecLogos.ITTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping() => Ok("Ticket controller is alive");
    }
}
