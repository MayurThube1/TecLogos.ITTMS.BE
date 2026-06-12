using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.DAL.Interfaces
{
    /// <summary>
    /// Data access contract for Admin dashboard operations.
    /// </summary>
    public interface IAdminDashboardRepository
    {
        /// <summary>
        /// Calls sp_GetAdminDashboardSummary.
        /// </summary>
        Task<AdminDashboardSummaryDTO?> GetDashboardSummaryAsync();

        /// <summary>
        /// Calls sp_GetAllEmployees. Returns paged result (count + rows).
        /// </summary>
        Task<PagedResultDTO<AdminEmployeeListDTO>> GetAllEmployeesAsync(string? department, bool? isActive, string? search, int pageNumber, int pageSize);

        /// <summary>
        /// Calls sp_GetEmployeeByIdAdmin.
        /// </summary>
        Task<AdminEmployeeDetailDTO?> GetEmployeeByIdAsync(Guid employeeId);

        /// <summary>
        /// Calls sp_ToggleEmployeeStatus. Toggles active/inactive and returns true if successful.
        /// </summary>
        Task<bool> ToggleEmployeeStatusAsync(Guid employeeId, Guid updatedById);

        /// <summary>
        /// Calls sp_GetAllTicketsAdmin. Returns paged result (count + rows).
        /// </summary>
        Task<PagedResultDTO<AdminTicketListDTO>> GetAllTicketsAsync(string? status, string? priority, string? department, Guid? assignedTo, int pageNumber, int pageSize);

        /// <summary>
        /// Calls sp_GetAdminTicketDetail. Returns ticket with comments and attachments.
        /// </summary>
        Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId);

        /// <summary>
        /// Calls sp_UpdateTicketStatusAdmin. Returns true if successful.
        /// </summary>
        Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, Guid updatedById, string? remarks);

        /// <summary>
        /// Calls sp_ReassignTicketAdmin. Returns true if successful.
        /// </summary>
        Task<bool> ReassignTicketAsync(Guid ticketId, Guid assignedToId, Guid reassignedById);

        /// <summary>
        /// Calls sp_GetAllAssets. Returns paged result (count + rows).
        /// </summary>
        Task<PagedResultDTO<AdminAssetListDTO>> GetAllAssetsAsync(string? assetType, string? status, string? search, int pageNumber, int pageSize);

        /// <summary>
        /// Calls sp_GetAssetDetail.
        /// </summary>
        Task<AdminAssetDetailDTO?> GetAssetDetailAsync(Guid assetId);

        /// <summary>
        /// Calls sp_GetAssetRequests. Returns paged result (count + rows).
        /// </summary>
        Task<PagedResultDTO<AdminAssetRequestListDTO>> GetAssetRequestsAsync(string? status, int pageNumber, int pageSize);

        /// <summary>
        /// Calls sp_UpdateAssetRequestStatus. Returns true if successful.
        /// </summary>
        Task<bool> UpdateAssetRequestStatusAsync(Guid requestId, string status, Guid reviewedById, string? remarks);

        /// <summary>
        /// Calls sp_GetDepartmentList.
        /// </summary>
        Task<IEnumerable<DepartmentLookupDTO>> GetDepartmentsAsync();

        /// <summary>
        /// Calls sp_GetRoleList.
        /// </summary>
        Task<IEnumerable<RoleLookupDTO>> GetRolesAsync();

        /// <summary>
        /// Calls sp_GetAllEngineersAdmin.
        /// </summary>
        Task<IEnumerable<EngineerLookupDTO>> GetEngineersAsync();
    }
}
