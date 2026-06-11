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
    /// Dapper-based repository for Team Lead dashboard data access using stored procedures and inline queries.
    /// </summary>
    public class TeamLeadDashboardRepository : ITeamLeadDashboardRepository
    {
        private readonly DBConnection _dbConnection;

        public TeamLeadDashboardRepository(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        /// <inheritdoc />
        public async Task<TeamLeadDashboardSummaryDTO?> GetDashboardSummaryAsync()
        {
            using var connection = _dbConnection.GetConnection();
            return await connection.QueryFirstOrDefaultAsync<TeamLeadDashboardSummaryDTO>(
                "sp_GetTeamLeadDashboardSummary",
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<TeamLeadTicketListDTO>> GetAllTicketsAsync(
            string? status, string? priority, Guid? assignedTo, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Status", status);
            parameters.Add("@Priority", priority);
            parameters.Add("@AssignedTo", assignedTo);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetAllTickets",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<TeamLeadTicketListDTO>()).ToList();

            return new PagedResultDTO<TeamLeadTicketListDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        /// <inheritdoc />
        public async Task<bool> AssignTicketAsync(Guid ticketId, Guid assignedToId, Guid assignedById)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TicketID", ticketId);
            parameters.Add("@AssignedToID", assignedToId);
            parameters.Add("@AssignedByID", assignedById);

            var rowsAffected = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_AssignTicket",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task<bool> EscalateTicketAsync(Guid ticketId, Guid escalatedById)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TicketID", ticketId);
            parameters.Add("@EscalatedByID", escalatedById);

            var rowsAffected = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_EscalateTicket",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EngineerWorkloadDTO>> GetEngineerWorkloadAsync()
        {
            using var connection = _dbConnection.GetConnection();
            return await connection.QueryAsync<EngineerWorkloadDTO>(
                "sp_GetEngineerWorkload",
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SlaOverviewDTO>> GetSlaOverviewAsync()
        {
            using var connection = _dbConnection.GetConnection();
            return await connection.QueryAsync<SlaOverviewDTO>(
                "sp_GetSlaOverview",
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EngineerLookupDTO>> GetEngineersLookupAsync()
        {
            using var connection = _dbConnection.GetConnection();
            string query = @"
                SELECT 
                    e.ID, 
                    CONCAT(e.FirstName, ' ', e.LastName) AS FullName 
                FROM Employees e 
                INNER JOIN EmployeeRoles er ON er.EmployeeID = e.ID 
                INNER JOIN [Role] r ON r.ID = er.RoleID 
                WHERE r.[Name] = 'IT Support Engineer' 
                  AND e.IsActive = 1 
                  AND e.IsDeleted = 0";

            return await connection.QueryAsync<EngineerLookupDTO>(query);
        }
    }
}
