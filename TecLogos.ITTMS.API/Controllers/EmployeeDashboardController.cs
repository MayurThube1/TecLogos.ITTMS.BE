using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.Models.DTOs.Dashboard;
using TecLogos.ITTMS.Models.Common;

namespace TecLogos.ITTMS.API.Controllers
{
    [ApiController]
    [Route("api/employee/dashboard")]
    [Authorize]
    [Produces("application/json")]
    public class EmployeeDashboardController : ControllerBase
    {
        private readonly IEmployeeDashboardService _dashboardService;
        private readonly ILogger<EmployeeDashboardController> _logger;

        public EmployeeDashboardController(IEmployeeDashboardService dashboardService, ILogger<EmployeeDashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Extracts the EmployeeID from the JWT 'sub' claim.
        /// </summary>
        private Guid GetEmployeeIdFromToken()
        {
            var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException("Invalid or missing employee identifier in token.");

            return employeeId;
        }

        /// <summary>
        /// GET /api/employee/dashboard/summary
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeDashboardSummaryDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var employeeId = GetEmployeeIdFromToken();
            _logger.LogInformation("Dashboard summary requested by EmployeeID: {EmployeeID}", employeeId);

            var summary = await _dashboardService.GetDashboardSummaryAsync(employeeId);

            if (summary == null)
                return NotFound(ApiResponse<object>.Fail("Dashboard summary not found."));

            return Ok(ApiResponse<EmployeeDashboardSummaryDTO>.Ok(summary, "Dashboard summary retrieved."));
        }

        /// <summary>
        /// GET /api/employee/dashboard/tickets?status=&amp;pageNumber=&amp;pageSize=
        /// </summary>
        [HttpGet("tickets")]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDTO<EmployeeTicketListDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetTickets([FromQuery] string? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var employeeId = GetEmployeeIdFromToken();
            _logger.LogInformation("Tickets requested by EmployeeID: {EmployeeID}, Status: {Status}, Page: {Page}", employeeId, status, pageNumber);

            var result = await _dashboardService.GetTicketsAsync(employeeId, status, pageNumber, pageSize);

            return Ok(ApiResponse<PagedResultDTO<EmployeeTicketListDTO>>.Ok(result, "Tickets retrieved."));
        }

        /// <summary>
        /// GET /api/employee/dashboard/tickets/{ticketId}
        /// </summary>
        [HttpGet("tickets/{ticketId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeTicketDetailDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetTicketDetail(Guid ticketId)
        {
            var employeeId = GetEmployeeIdFromToken();
            _logger.LogInformation("Ticket detail requested. TicketID: {TicketID}, EmployeeID: {EmployeeID}", ticketId, employeeId);

            var detail = await _dashboardService.GetTicketDetailAsync(ticketId, employeeId);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail("Ticket not found or access denied."));

            return Ok(ApiResponse<EmployeeTicketDetailDTO>.Ok(detail, "Ticket detail retrieved."));
        }

        /// <summary>
        /// POST /api/employee/dashboard/tickets
        /// </summary>
        [HttpPost("tickets")]
        [ProducesResponseType(typeof(ApiResponse<RaiseTicketResponseDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<RaiseTicketResponseDTO>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RaiseTicket([FromBody] RaiseTicketRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            var employeeId = GetEmployeeIdFromToken();
            _logger.LogInformation("Raising ticket for EmployeeID: {EmployeeID}, Subject: {Subject}", employeeId, request.Subject);

            var result = await _dashboardService.RaiseTicketAsync(employeeId, request);

            if (result == null)
                return StatusCode(500, ApiResponse<object>.Fail("Failed to raise ticket."));

            _logger.LogInformation("Ticket raised successfully. Number: {Number}", result.Number);
            return CreatedAtAction(nameof(GetTicketDetail), new { ticketId = result.Id },
                ApiResponse<RaiseTicketResponseDTO>.Ok(result, "Ticket raised successfully."));
        }

        /// <summary>
        /// GET /api/employee/dashboard/assets
        /// </summary>
        [HttpGet("assets")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeAssetDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAssets()
        {
            var employeeId = GetEmployeeIdFromToken();
            _logger.LogInformation("Assets requested by EmployeeID: {EmployeeID}", employeeId);

            var assets = await _dashboardService.GetAssetsAsync(employeeId);

            return Ok(ApiResponse<IEnumerable<EmployeeAssetDTO>>.Ok(assets, "Assets retrieved."));
        }

        /// <summary>
        /// GET /api/employee/dashboard/profile
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeProfileDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetProfile()
        {
            var employeeId = GetEmployeeIdFromToken();
            _logger.LogInformation("Profile requested by EmployeeID: {EmployeeID}", employeeId);

            var profile = await _dashboardService.GetProfileAsync(employeeId);

            if (profile == null)
                return NotFound(ApiResponse<object>.Fail("Employee profile not found."));

            return Ok(ApiResponse<EmployeeProfileDTO>.Ok(profile, "Profile retrieved."));
        }

        /// <summary>
        /// POST /api/employee/dashboard/tickets/{ticketId}/feedback
        /// </summary>
        [HttpPost("tickets/{ticketId:guid}/feedback")]
        [ProducesResponseType(typeof(ApiResponse<SubmitFeedbackResponseDTO>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> SubmitFeedback(Guid ticketId, [FromBody] SubmitFeedbackRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            var employeeId = GetEmployeeIdFromToken();
            _logger.LogInformation("Feedback submitted for TicketID: {TicketID} by EmployeeID: {EmployeeID}", ticketId, employeeId);

            var result = await _dashboardService.SubmitFeedbackAsync(ticketId, employeeId, request);

            if (result == null)
                return StatusCode(500, ApiResponse<object>.Fail("Failed to submit feedback."));

            if (!result.Success)
                return BadRequest(ApiResponse<SubmitFeedbackResponseDTO>.Fail(result.Message));

            return Ok(ApiResponse<SubmitFeedbackResponseDTO>.Ok(result, result.Message));
        }
    }
}
