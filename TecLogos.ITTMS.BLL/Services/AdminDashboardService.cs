using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Services
{
    /// <summary>
    /// Business logic service implementing Admin dashboard operations.
    /// </summary>
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _repository;

        public AdminDashboardService(IAdminDashboardRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<AdminDashboardSummaryDTO?> GetDashboardSummaryAsync()
        {
            return await _repository.GetDashboardSummaryAsync();
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AdminEmployeeListDTO>> GetAllEmployeesAsync(
            string? department, bool? isActive, string? search, int pageNumber, int pageSize)
        {
            return await _repository.GetAllEmployeesAsync(department, isActive, search, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<AdminEmployeeDetailDTO?> GetEmployeeByIdAsync(Guid employeeId)
        {
            return await _repository.GetEmployeeByIdAsync(employeeId);
        }

        /// <inheritdoc />
        public async Task<bool> ToggleEmployeeStatusAsync(Guid employeeId, Guid updatedById)
        {
            return await _repository.ToggleEmployeeStatusAsync(employeeId, updatedById);
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AdminTicketListDTO>> GetAllTicketsAsync(
            string? status, string? priority, string? department, Guid? assignedTo, int pageNumber, int pageSize)
        {
            return await _repository.GetAllTicketsAsync(status, priority, department, assignedTo, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId)
        {
            return await _repository.GetTicketDetailAsync(ticketId);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, Guid updatedById, string? remarks)
        {
            return await _repository.UpdateTicketStatusAsync(ticketId, status, updatedById, remarks);
        }

        /// <inheritdoc />
        public async Task<bool> ReassignTicketAsync(Guid ticketId, Guid assignedToId, Guid reassignedById)
        {
            return await _repository.ReassignTicketAsync(ticketId, assignedToId, reassignedById);
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AdminAssetListDTO>> GetAllAssetsAsync(
            string? assetType, string? status, string? search, int pageNumber, int pageSize)
        {
            return await _repository.GetAllAssetsAsync(assetType, status, search, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<AdminAssetDetailDTO?> GetAssetDetailAsync(Guid assetId)
        {
            return await _repository.GetAssetDetailAsync(assetId);
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AdminAssetRequestListDTO>> GetAssetRequestsAsync(
            string? status, int pageNumber, int pageSize)
        {
            return await _repository.GetAssetRequestsAsync(status, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAssetRequestStatusAsync(Guid requestId, string status, Guid reviewedById, string? remarks)
        {
            return await _repository.UpdateAssetRequestStatusAsync(requestId, status, reviewedById, remarks);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DepartmentLookupDTO>> GetDepartmentsAsync()
        {
            return await _repository.GetDepartmentsAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RoleLookupDTO>> GetRolesAsync()
        {
            return await _repository.GetRolesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EngineerLookupDTO>> GetEngineersAsync()
        {
            return await _repository.GetEngineersAsync();
        }
    }
}
