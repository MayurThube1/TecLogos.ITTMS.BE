using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.Models.Common;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.API.Controllers
{
    [ApiController]
    [Route("api/teamlead/dashboard")]
    [Authorize]
    [Produces("application/json")]
    public class TeamLeadDashboardController : ControllerBase
    {
        private readonly ITeamLeadDashboardService _dashboardService;
        private readonly ILogger<TeamLeadDashboardController> _logger;

        public TeamLeadDashboardController(ITeamLeadDashboardService dashboardService, ILogger<TeamLeadDashboardController> logger)
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
        /// GET /api/teamlead/dashboard/summary
        /// Retrieves top-level card stats for the Team Lead Dashboard.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<TeamLeadDashboardSummaryDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(550)]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var leadId = GetEmployeeIdFromToken();
            _logger.LogInformation("Team Lead dashboard summary requested by LeadID: {LeadID}", leadId);

            var summary = await _dashboardService.GetDashboardSummaryAsync();

            if (summary == null)
                return NotFound(ApiResponse<object>.Fail("Team Lead dashboard summary not found."));

            return Ok(ApiResponse<TeamLeadDashboardSummaryDTO>.Ok(summary, "Dashboard summary retrieved."));
        }

        /// <summary>
        /// GET /api/teamlead/dashboard/tickets
        /// Retrieves all support tickets with optional status, priority, and assigned engineer filters.
        /// </summary>
        [HttpGet("tickets")]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDTO<TeamLeadTicketListDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetTickets(
            [FromQuery] string? status, 
            [FromQuery] string? priority, 
            [FromQuery] Guid? assignedTo, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 15)
        {
            var leadId = GetEmployeeIdFromToken();
            _logger.LogInformation("All tickets requested by LeadID: {LeadID}. Status: {Status}, Priority: {Priority}, AssignedTo: {AssignedTo}", leadId, status, priority, assignedTo);

            var result = await _dashboardService.GetAllTicketsAsync(status, priority, assignedTo, pageNumber, pageSize);

            return Ok(ApiResponse<PagedResultDTO<TeamLeadTicketListDTO>>.Ok(result, "Tickets retrieved."));
        }

        /// <summary>
        /// POST /api/teamlead/dashboard/tickets/{ticketId}/assign
        /// Assigns a ticket to a support engineer.
        /// </summary>
        [HttpPost("tickets/{ticketId:guid}/assign")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignTicket(Guid ticketId, [FromBody] AssignTicketRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            var leadId = GetEmployeeIdFromToken();
            _logger.LogInformation("Assigning ticket {TicketID} to engineer {EngineerID} by LeadID: {LeadID}", ticketId, request.AssignedToID, leadId);

            var success = await _dashboardService.AssignTicketAsync(ticketId, request.AssignedToID, leadId);

            if (!success)
                return NotFound(ApiResponse<object>.Fail("Failed to assign ticket. Ticket may not exist or has been deleted."));

            return Ok(ApiResponse<object>.Ok(null, "Ticket successfully assigned."));
        }

        /// <summary>
        /// POST /api/teamlead/dashboard/tickets/{ticketId}/escalate
        /// Escalates a ticket priority to Critical.
        /// </summary>
        [HttpPost("tickets/{ticketId:guid}/escalate")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EscalateTicket(Guid ticketId)
        {
            var leadId = GetEmployeeIdFromToken();
            _logger.LogInformation("Escalating ticket {TicketID} by LeadID: {LeadID}", ticketId, leadId);

            var success = await _dashboardService.EscalateTicketAsync(ticketId, leadId);

            if (!success)
                return NotFound(ApiResponse<object>.Fail("Failed to escalate ticket. Ticket may not exist or has been deleted."));

            return Ok(ApiResponse<object>.Ok(null, "Ticket successfully escalated to Critical priority."));
        }

        /// <summary>
        /// GET /api/teamlead/dashboard/workload
        /// Retrieves ticket workload information for all support engineers.
        /// </summary>
        [HttpGet("workload")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EngineerWorkloadDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetWorkload()
        {
            var leadId = GetEmployeeIdFromToken();
            _logger.LogInformation("Engineer workloads requested by LeadID: {LeadID}", leadId);

            var workload = await _dashboardService.GetEngineerWorkloadAsync();

            return Ok(ApiResponse<IEnumerable<EngineerWorkloadDTO>>.Ok(workload, "Engineer workloads retrieved."));
        }

        /// <summary>
        /// GET /api/teamlead/dashboard/sla
        /// Retrieves SLA breach tracking overview for active tickets.
        /// </summary>
        [HttpGet("sla")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SlaOverviewDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetSlaOverview()
        {
            var leadId = GetEmployeeIdFromToken();
            _logger.LogInformation("SLA overview requested by LeadID: {LeadID}", leadId);

            var slaOverview = await _dashboardService.GetSlaOverviewAsync();

            return Ok(ApiResponse<IEnumerable<SlaOverviewDTO>>.Ok(slaOverview, "SLA overview retrieved."));
        }

        /// <summary>
        /// GET /api/teamlead/dashboard/engineers
        /// Retrieves the list of support engineers for ticket assignment dropdown.
        /// </summary>
        [HttpGet("engineers")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EngineerLookupDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetEngineersLookup()
        {
            var leadId = GetEmployeeIdFromToken();
            _logger.LogInformation("Engineers lookup requested by LeadID: {LeadID}", leadId);

            var engineers = await _dashboardService.GetEngineersLookupAsync();

            return Ok(ApiResponse<IEnumerable<EngineerLookupDTO>>.Ok(engineers, "Engineers lookup retrieved."));
        }

        /// <summary>
        /// GET /api/teamlead/dashboard/tickets/{ticketId}
        /// Retrieves full details for a ticket (accessible to Team Lead and Administrator).
        /// </summary>
        [HttpGet("tickets/{ticketId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeTicketDetailDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTicketDetail(Guid ticketId)
        {
            var leadId = GetEmployeeIdFromToken();
            _logger.LogInformation("Ticket detail requested for TicketID: {TicketID} by LeadID: {LeadID}", ticketId, leadId);

            var detail = await _dashboardService.GetTicketDetailAsync(ticketId);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail("Ticket not found or access denied."));

            return Ok(ApiResponse<EmployeeTicketDetailDTO>.Ok(detail, "Ticket detail retrieved."));
        }
    }
}
