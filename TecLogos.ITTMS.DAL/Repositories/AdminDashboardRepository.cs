using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TecLogos.ITTMS.DAL.DBHelper;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.DAL.Repositories
{
    /// <summary>
    /// Dapper-based repository for Admin dashboard data access using stored procedures.
    /// </summary>
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly DBConnection _dbConnection;

        public AdminDashboardRepository(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        /// <inheritdoc />
        public async Task<AdminDashboardSummaryDTO?> GetDashboardSummaryAsync()
        {
            using var connection = _dbConnection.GetConnection();
            return await connection.QueryFirstOrDefaultAsync<AdminDashboardSummaryDTO>(
                "sp_GetAdminDashboardSummary",
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AdminEmployeeListDTO>> GetAllEmployeesAsync(
            string? department, bool? isActive, string? search, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Department", department);
            parameters.Add("@IsActive", isActive);
            parameters.Add("@Search", search);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetAllEmployees",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<AdminEmployeeListDTO>()).ToList();

            return new PagedResultDTO<AdminEmployeeListDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        /// <inheritdoc />
        public async Task<AdminEmployeeDetailDTO?> GetEmployeeByIdAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);

            return await connection.QueryFirstOrDefaultAsync<AdminEmployeeDetailDTO>(
                "sp_GetEmployeeByIdAdmin",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<bool> ToggleEmployeeStatusAsync(Guid employeeId, Guid updatedById)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);
            parameters.Add("@UpdatedByID", updatedById);

            var rowsAffected = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_ToggleEmployeeStatus",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AdminTicketListDTO>> GetAllTicketsAsync(
            string? status, string? priority, string? department, Guid? assignedTo, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Status", status);
            parameters.Add("@Priority", priority);
            parameters.Add("@Department", department);
            parameters.Add("@AssignedTo", assignedTo);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetAllTicketsAdmin",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<AdminTicketListDTO>()).ToList();

            return new PagedResultDTO<AdminTicketListDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        /// <inheritdoc />
        public async Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TicketID", ticketId);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetAdminTicketDetail",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var ticket = await multi.ReadFirstOrDefaultAsync<EmployeeTicketDetailDTO>();
            if (ticket == null)
                return null;

            ticket.Comments = (await multi.ReadAsync<TicketCommentDTO>()).ToList();
            ticket.Attachments = (await multi.ReadAsync<TicketAttachmentDTO>()).ToList();

            return ticket;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, Guid updatedById, string? remarks)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TicketID", ticketId);
            parameters.Add("@Status", status);
            parameters.Add("@UpdatedByID", updatedById);
            parameters.Add("@Remarks", remarks);

            var rowsAffected = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_UpdateTicketStatusAdmin",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task<bool> ReassignTicketAsync(Guid ticketId, Guid assignedToId, Guid reassignedById)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TicketID", ticketId);
            parameters.Add("@AssignedToID", assignedToId);
            parameters.Add("@ReassignedByID", reassignedById);

            var rowsAffected = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_ReassignTicketAdmin",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AdminAssetListDTO>> GetAllAssetsAsync(
            string? assetType, string? status, string? search, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@AssetType", assetType);
            parameters.Add("@Status", status);
            parameters.Add("@Search", search);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetAllAssets",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<AdminAssetListDTO>()).ToList();

            return new PagedResultDTO<AdminAssetListDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        /// <inheritdoc />
        public async Task<AdminAssetDetailDTO?> GetAssetDetailAsync(Guid assetId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@AssetID", assetId);

            return await connection.QueryFirstOrDefaultAsync<AdminAssetDetailDTO>(
                "sp_GetAssetDetail",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AdminAssetRequestListDTO>> GetAssetRequestsAsync(
            string? status, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Status", status);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetAssetRequests",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<AdminAssetRequestListDTO>()).ToList();

            return new PagedResultDTO<AdminAssetRequestListDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAssetRequestStatusAsync(Guid requestId, string status, Guid reviewedById, string? remarks)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@RequestID", requestId);
            parameters.Add("@Status", status);
            parameters.Add("@ReviewedByID", reviewedById);
            parameters.Add("@Remarks", remarks);

            var rowsAffected = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_UpdateAssetRequestStatus",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DepartmentLookupDTO>> GetDepartmentsAsync()
        {
            using var connection = _dbConnection.GetConnection();
            return await connection.QueryAsync<DepartmentLookupDTO>(
                "sp_GetDepartmentList",
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RoleLookupDTO>> GetRolesAsync()
        {
            using var connection = _dbConnection.GetConnection();
            return await connection.QueryAsync<RoleLookupDTO>(
                "sp_GetRoleList",
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EngineerLookupDTO>> GetEngineersAsync()
        {
            using var connection = _dbConnection.GetConnection();
            return await connection.QueryAsync<EngineerLookupDTO>(
                "sp_GetAllEngineersAdmin",
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
