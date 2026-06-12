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
    [Route("api/admin")]
    [Authorize]
    [Produces("application/json")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(IAdminDashboardService dashboardService, ILogger<AdminDashboardController> logger)
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

        // =============================================
        // Dashboard Summary
        // =============================================

        /// <summary>
        /// GET /api/admin/dashboard/summary
        /// Retrieves top-level summary card stats for the Admin Dashboard.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AdminDashboardSummaryDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Admin dashboard summary requested by AdminID: {AdminID}", adminId);

            var summary = await _dashboardService.GetDashboardSummaryAsync();

            if (summary == null)
                return NotFound(ApiResponse<object>.Fail("Admin dashboard summary not found."));

            return Ok(ApiResponse<AdminDashboardSummaryDTO>.Ok(summary, "Dashboard summary retrieved."));
        }

        // =============================================
        // Employee Management
        // =============================================

        /// <summary>
        /// GET /api/admin/dashboard/employees
        /// Retrieves all employees with optional department, status, and search filters.
        /// </summary>
        [HttpGet("employees")]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDTO<AdminEmployeeListDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAllEmployees(
            [FromQuery] string? department,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15)
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("All employees requested by AdminID: {AdminID}. Department: {Department}, IsActive: {IsActive}, Search: {Search}",
                adminId, department, isActive, search);

            var result = await _dashboardService.GetAllEmployeesAsync(department, isActive, search, pageNumber, pageSize);

            return Ok(ApiResponse<PagedResultDTO<AdminEmployeeListDTO>>.Ok(result, "Employees retrieved."));
        }

        /// <summary>
        /// GET /api/admin/dashboard/employees/{employeeId}
        /// Retrieves detailed information for a specific employee.
        /// </summary>
        [HttpGet("employees/{employeeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AdminEmployeeDetailDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetEmployeeById(Guid employeeId)
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Employee detail requested for EmployeeID: {EmployeeID} by AdminID: {AdminID}", employeeId, adminId);

            var employee = await _dashboardService.GetEmployeeByIdAsync(employeeId);

            if (employee == null)
                return NotFound(ApiResponse<object>.Fail("Employee not found."));

            return Ok(ApiResponse<AdminEmployeeDetailDTO>.Ok(employee, "Employee detail retrieved."));
        }

        /// <summary>
        /// POST /api/admin/dashboard/employees/{employeeId}/toggle-status
        /// Toggles an employee's active/inactive status.
        /// </summary>
        [HttpPost("employees/{employeeId:guid}/toggle-status")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ToggleEmployeeStatus(Guid employeeId)
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Toggle employee status for EmployeeID: {EmployeeID} by AdminID: {AdminID}", employeeId, adminId);

            var success = await _dashboardService.ToggleEmployeeStatusAsync(employeeId, adminId);

            if (!success)
                return NotFound(ApiResponse<object>.Fail("Failed to toggle employee status. Employee may not exist."));

            return Ok(ApiResponse<object>.Ok(null, "Employee status toggled successfully."));
        }

        // =============================================
        // Ticket Management
        // =============================================

        /// <summary>
        /// GET /api/admin/dashboard/tickets
        /// Retrieves all tickets with optional status, priority, department, and engineer filters.
        /// </summary>
        [HttpGet("tickets")]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDTO<AdminTicketListDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAllTickets(
            [FromQuery] string? status,
            [FromQuery] string? priority,
            [FromQuery] string? department,
            [FromQuery] Guid? assignedTo,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15)
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("All tickets requested by AdminID: {AdminID}. Status: {Status}, Priority: {Priority}, Department: {Department}, AssignedTo: {AssignedTo}",
                adminId, status, priority, department, assignedTo);

            var result = await _dashboardService.GetAllTicketsAsync(status, priority, department, assignedTo, pageNumber, pageSize);

            return Ok(ApiResponse<PagedResultDTO<AdminTicketListDTO>>.Ok(result, "Tickets retrieved."));
        }

        /// <summary>
        /// GET /api/admin/dashboard/tickets/{ticketId}
        /// Retrieves full ticket detail including comments and attachments.
        /// </summary>
        [HttpGet("tickets/{ticketId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeTicketDetailDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTicketDetail(Guid ticketId)
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Ticket detail requested for TicketID: {TicketID} by AdminID: {AdminID}", ticketId, adminId);

            var detail = await _dashboardService.GetTicketDetailAsync(ticketId);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail("Ticket not found."));

            return Ok(ApiResponse<EmployeeTicketDetailDTO>.Ok(detail, "Ticket detail retrieved."));
        }

        /// <summary>
        /// POST /api/admin/dashboard/tickets/{ticketId}/update-status
        /// Updates a ticket's status (admin override).
        /// </summary>
        [HttpPost("tickets/{ticketId:guid}/update-status")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTicketStatus(Guid ticketId, [FromBody] UpdateTicketStatusRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Update ticket status for TicketID: {TicketID} to {Status} by AdminID: {AdminID}", ticketId, request.Status, adminId);

            var success = await _dashboardService.UpdateTicketStatusAsync(ticketId, request.Status, adminId, request.Remarks);

            if (!success)
                return NotFound(ApiResponse<object>.Fail("Failed to update ticket status. Ticket may not exist."));

            return Ok(ApiResponse<object>.Ok(null, "Ticket status updated successfully."));
        }

        /// <summary>
        /// POST /api/admin/dashboard/tickets/{ticketId}/reassign
        /// Reassigns a ticket to a different engineer.
        /// </summary>
        [HttpPost("tickets/{ticketId:guid}/reassign")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ReassignTicket(Guid ticketId, [FromBody] ReassignTicketRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Reassigning ticket {TicketID} to engineer {EngineerID} by AdminID: {AdminID}", ticketId, request.AssignedToID, adminId);

            var success = await _dashboardService.ReassignTicketAsync(ticketId, request.AssignedToID, adminId);

            if (!success)
                return NotFound(ApiResponse<object>.Fail("Failed to reassign ticket. Ticket may not exist."));

            return Ok(ApiResponse<object>.Ok(null, "Ticket reassigned successfully."));
        }

        // =============================================
        // Asset Management
        // =============================================

        /// <summary>
        /// GET /api/admin/dashboard/assets
        /// Retrieves all assets with optional type, status, and search filters.
        /// </summary>
        [HttpGet("assets")]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDTO<AdminAssetListDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAllAssets(
            [FromQuery] string? assetType,
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15)
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("All assets requested by AdminID: {AdminID}. AssetType: {AssetType}, Status: {Status}, Search: {Search}",
                adminId, assetType, status, search);

            var result = await _dashboardService.GetAllAssetsAsync(assetType, status, search, pageNumber, pageSize);

            return Ok(ApiResponse<PagedResultDTO<AdminAssetListDTO>>.Ok(result, "Assets retrieved."));
        }

        /// <summary>
        /// GET /api/admin/dashboard/assets/{assetId}
        /// Retrieves detailed information for a specific asset.
        /// </summary>
        [HttpGet("assets/{assetId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AdminAssetDetailDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAssetDetail(Guid assetId)
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Asset detail requested for AssetID: {AssetID} by AdminID: {AdminID}", assetId, adminId);

            var asset = await _dashboardService.GetAssetDetailAsync(assetId);

            if (asset == null)
                return NotFound(ApiResponse<object>.Fail("Asset not found."));

            return Ok(ApiResponse<AdminAssetDetailDTO>.Ok(asset, "Asset detail retrieved."));
        }

        /// <summary>
        /// GET /api/admin/dashboard/asset-requests
        /// Retrieves asset requests with optional status filter.
        /// </summary>
        [HttpGet("asset-requests")]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDTO<AdminAssetRequestListDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAssetRequests(
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15)
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Asset requests requested by AdminID: {AdminID}. Status: {Status}", adminId, status);

            var result = await _dashboardService.GetAssetRequestsAsync(status, pageNumber, pageSize);

            return Ok(ApiResponse<PagedResultDTO<AdminAssetRequestListDTO>>.Ok(result, "Asset requests retrieved."));
        }

        /// <summary>
        /// POST /api/admin/dashboard/asset-requests/{requestId}/update-status
        /// Approves or rejects an asset request.
        /// </summary>
        [HttpPost("asset-requests/{requestId:guid}/update-status")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAssetRequestStatus(Guid requestId, [FromBody] UpdateAssetRequestStatusDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Update asset request status for RequestID: {RequestID} to {Status} by AdminID: {AdminID}", requestId, request.Status, adminId);

            var success = await _dashboardService.UpdateAssetRequestStatusAsync(requestId, request.Status, adminId, request.Remarks);

            if (!success)
                return NotFound(ApiResponse<object>.Fail("Failed to update asset request status. Request may not exist."));

            return Ok(ApiResponse<object>.Ok(null, "Asset request status updated successfully."));
        }

        // =============================================
        // Lookup Endpoints
        // =============================================

        /// <summary>
        /// GET /api/admin/dashboard/departments
        /// Retrieves the list of departments for filter dropdowns.
        /// </summary>
        [HttpGet("departments")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DepartmentLookupDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDepartments()
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Departments lookup requested by AdminID: {AdminID}", adminId);

            var departments = await _dashboardService.GetDepartmentsAsync();

            return Ok(ApiResponse<IEnumerable<DepartmentLookupDTO>>.Ok(departments, "Departments retrieved."));
        }

        /// <summary>
        /// GET /api/admin/dashboard/roles
        /// Retrieves the list of roles for filter dropdowns.
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleLookupDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetRoles()
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Roles lookup requested by AdminID: {AdminID}", adminId);

            var roles = await _dashboardService.GetRolesAsync();

            return Ok(ApiResponse<IEnumerable<RoleLookupDTO>>.Ok(roles, "Roles retrieved."));
        }

        /// <summary>
        /// GET /api/admin/dashboard/engineers
        /// Retrieves the list of support engineers for ticket reassignment dropdown.
        /// </summary>
        [HttpGet("engineers")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EngineerLookupDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetEngineers()
        {
            var adminId = GetEmployeeIdFromToken();
            _logger.LogInformation("Engineers lookup requested by AdminID: {AdminID}", adminId);

            var engineers = await _dashboardService.GetEngineersAsync();

            return Ok(ApiResponse<IEnumerable<EngineerLookupDTO>>.Ok(engineers, "Engineers retrieved."));
        }
    }
}
