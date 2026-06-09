using Microsoft.AspNetCore.Mvc;

namespace TecLogos.ITTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        // TODO: Inject IEmployeeService and implement endpoints

        [HttpGet("ping")]
        public IActionResult Ping() => Ok("Employee controller is alive");
    }
}
