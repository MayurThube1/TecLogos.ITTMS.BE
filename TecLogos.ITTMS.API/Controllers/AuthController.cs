using Microsoft.AspNetCore.Mvc;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.Common;



namespace TecLogos.ITTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates an employee with email and password.
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            // Get client IP address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            _logger.LogInformation("Login attempt for email: {Email} from IP: {IP}", request.Email, ipAddress);

            var result = await _authService.AuthenticateAsync(request, ipAddress);

            if (result == null)
            {
                _logger.LogWarning("Failed login attempt for email: {Email} from IP: {IP}", request.Email, ipAddress);
                return Unauthorized(ApiResponse<object>.Fail("Invalid email or password."));
            }

            _logger.LogInformation("Successful login for email: {Email}", request.Email);
            return Ok(ApiResponse<LoginResponseDTO>.Ok(result, "Login successful."));
        }
    }
}
