using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for Admin dashboard operations.
    /// </summary>
    public interface IAdminDashboardService
    {
        Task<AdminDashboardSummaryDTO?> GetDashboardSummaryAsync();
        Task<PagedResultDTO<AdminEmployeeListDTO>> GetAllEmployeesAsync(string? department, bool? isActive, string? search, int pageNumber, int pageSize);
        Task<AdminEmployeeDetailDTO?> GetEmployeeByIdAsync(Guid employeeId);
        Task<bool> ToggleEmployeeStatusAsync(Guid employeeId, Guid updatedById);
        Task<PagedResultDTO<AdminTicketListDTO>> GetAllTicketsAsync(string? status, string? priority, string? department, Guid? assignedTo, int pageNumber, int pageSize);
        Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId);
        Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, Guid updatedById, string? remarks);
        Task<bool> ReassignTicketAsync(Guid ticketId, Guid assignedToId, Guid reassignedById);
        Task<PagedResultDTO<AdminAssetListDTO>> GetAllAssetsAsync(string? assetType, string? status, string? search, int pageNumber, int pageSize);
        Task<AdminAssetDetailDTO?> GetAssetDetailAsync(Guid assetId);
        Task<PagedResultDTO<AdminAssetRequestListDTO>> GetAssetRequestsAsync(string? status, int pageNumber, int pageSize);
        Task<bool> UpdateAssetRequestStatusAsync(Guid requestId, string status, Guid reviewedById, string? remarks);
        Task<IEnumerable<DepartmentLookupDTO>> GetDepartmentsAsync();
        Task<IEnumerable<RoleLookupDTO>> GetRolesAsync();
        Task<IEnumerable<EngineerLookupDTO>> GetEngineersAsync();
    }
}
