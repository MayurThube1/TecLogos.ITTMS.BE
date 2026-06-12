using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.Common;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ITSupportEngineerController : ControllerBase
    {
        private readonly IITSupportEngineerService _supportService;
        private readonly ILogger<ITSupportEngineerController> _logger;

        public ITSupportEngineerController(IITSupportEngineerService supportService, ILogger<ITSupportEngineerController> logger)
        {
            _supportService = supportService;
            _logger = logger;
        }

        private Guid GetEmployeeIdFromToken()
        {
            var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException("Invalid or missing employee identifier in token.");

            return employeeId;
        }

        // 1. View Assigned Tickets
        [HttpGet("assigned-tickets")]
        public async Task<IActionResult> GetAssignedTickets([FromQuery] string? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Assigned tickets requested by EngineerID: {EngineerID}, Status: {Status}", engineerId, status);

                var result = await _supportService.GetAssignedTicketsAsync(engineerId, status, pageNumber, pageSize);
                return Ok(ApiResponse<PagedResultDTO<AssignedTicketDTO>>.Ok(result, "Assigned tickets retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assigned tickets.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while retrieving assigned tickets."));
            }
        }

        // 2. Update Ticket Status
        [HttpPut("ticket-status")]
        public async Task<IActionResult> UpdateTicketStatus([FromBody] UpdateTicketStatusRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Status update requested for Ticket: {TicketId} to {Status}", request.TicketId, request.Status);

                var success = await _supportService.UpdateTicketStatusAsync(request.TicketId, request.Status, request.Remarks, engineerId);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Ticket not found or update failed."));

                return Ok(ApiResponse<bool>.Ok(true, "Ticket status updated successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket status.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while updating the ticket status."));
            }
        }

        // 3. Add Work Notes
        [HttpPost("work-notes")]
        public async Task<IActionResult> AddWorkNote([FromBody] AddWorkNoteRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Adding work note to Ticket: {TicketId}", request.TicketId);

                var success = await _supportService.AddWorkNoteAsync(request.TicketId, request.NoteText, engineerId);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Ticket not found or adding work note failed."));

                return Ok(ApiResponse<bool>.Ok(true, "Work note added successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding work note.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while adding the work note."));
            }
        }

        // 4. Add Resolution Details
        [HttpPost("resolution")]
        public async Task<IActionResult> AddResolution([FromBody] AddResolutionRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Resolving Ticket: {TicketId}", request.TicketId);

                var success = await _supportService.AddResolutionAsync(request.TicketId, request.ResolutionText, engineerId);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Ticket not found or resolution failed."));

                return Ok(ApiResponse<bool>.Ok(true, "Ticket resolved successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving ticket.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while resolving the ticket."));
            }
        }

        // 5. Escalate Ticket
        [HttpPost("escalate")]
        public async Task<IActionResult> EscalateTicket([FromBody] EscalateTicketRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Escalating Ticket: {TicketId}", request.TicketId);

                var success = await _supportService.EscalateTicketAsync(request.TicketId, request.Reason, engineerId);
                if (!success)
                    return BadRequest(ApiResponse<object>.Fail("Ticket escalation failed. No manager or supervisor found to escalate to."));

                return Ok(ApiResponse<bool>.Ok(true, "Ticket escalated successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escalating ticket.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while escalating the ticket."));
            }
        }

        // 6. Close Ticket
        [HttpPut("close-ticket")]
        public async Task<IActionResult> CloseTicket([FromBody] CloseTicketRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Closing Ticket: {TicketId}", request.TicketId);

                var success = await _supportService.CloseTicketAsync(request.TicketId, engineerId);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Ticket not found or closing failed."));

                return Ok(ApiResponse<bool>.Ok(true, "Ticket closed successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing ticket.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while closing the ticket."));
            }
        }

        // 7. Schedule Appointment / Site Visit
        [HttpPost("schedule-appointment")]
        public async Task<IActionResult> ScheduleAppointment([FromBody] ScheduleAppointmentRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Scheduling appointment for Ticket: {TicketId} at {Time}", request.TicketId, request.AppointmentTime);

                var success = await _supportService.ScheduleAppointmentAsync(request.TicketId, request.AppointmentTime, request.Description, engineerId);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Ticket not found or scheduling failed."));

                return Ok(ApiResponse<bool>.Ok(true, "Appointment scheduled successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling appointment.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while scheduling the appointment."));
            }
        }

        // 8. Allocate Asset
        [HttpPost("allocate-asset")]
        public async Task<IActionResult> AllocateAsset([FromBody] AllocateAssetRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Allocating asset {AssetId} to Employee {EmployeeId}", request.AssetId, request.EmployeeId);

                var success = await _supportService.AllocateAssetAsync(request.AssetId, request.EmployeeId, request.Remarks, engineerId);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Asset not found or allocation failed."));

                return Ok(ApiResponse<bool>.Ok(true, "Asset allocated successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating asset.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while allocating the asset."));
            }
        }

        // 9. Return Asset
        [HttpPost("return-asset")]
        public async Task<IActionResult> ReturnAsset([FromBody] ReturnAssetRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("Returning asset {AssetId}", request.AssetId);

                var success = await _supportService.ReturnAssetAsync(request.AssetId, request.Remarks, engineerId);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Asset not found or return failed."));

                return Ok(ApiResponse<bool>.Ok(true, "Asset returned successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning asset.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while returning the asset."));
            }
        }

        // 10. Track SLA Compliance
        [HttpGet("sla-compliance")]
        public async Task<IActionResult> GetSlaCompliance()
        {
            try
            {
                var engineerId = GetEmployeeIdFromToken();
                _logger.LogInformation("SLA compliance stats requested by EngineerID: {EngineerID}", engineerId);

                var result = await _supportService.GetSlaComplianceAsync(engineerId);
                return Ok(ApiResponse<SlaComplianceDTO>.Ok(result, "SLA compliance stats retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SLA compliance.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while retrieving SLA compliance stats."));
            }
        }
    }
}
