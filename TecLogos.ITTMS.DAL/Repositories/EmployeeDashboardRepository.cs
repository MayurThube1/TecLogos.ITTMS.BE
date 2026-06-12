using Dapper;
using System.Data;
using TecLogos.ITTMS.DAL.DBHelper;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.DAL.Repositories
{
    /// <summary>
    /// Dapper-based repository for employee dashboard data access using stored procedures.
    /// </summary>
    public class EmployeeDashboardRepository : IEmployeeDashboardRepository
    {
        private readonly DBConnection _dbConnection;

        public EmployeeDashboardRepository(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        /// <inheritdoc />
        public async Task<EmployeeDashboardSummaryDTO?> GetDashboardSummaryAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<EmployeeDashboardSummaryDTO>(
                "sp_GetEmployeeDashboardSummary",
                new { EmployeeID = employeeId },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<EmployeeTicketListDTO>> GetTicketsAsync(Guid employeeId, string? status, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetEmployeeTickets",
                new
                {
                    EmployeeID = employeeId,
                    Status = status,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                commandType: CommandType.StoredProcedure
            );

            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<EmployeeTicketListDTO>()).ToList();

            return new PagedResultDTO<EmployeeTicketListDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        /// <inheritdoc />
        public async Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId, Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetEmployeeTicketDetail",
                new { TicketID = ticketId, EmployeeID = employeeId },
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
        public async Task<RaiseTicketResponseDTO?> RaiseTicketAsync(Guid employeeId, RaiseTicketRequestDTO request)
        {
            using var connection = _dbConnection.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<RaiseTicketResponseDTO>(
                "sp_RaiseTicket",
                new
                {
                    EmployeeID = employeeId,
                    Category = request.Category,
                    SubCategory = request.SubCategory,
                    Priority = request.Priority,
                    Subject = request.Subject,
                    Description = request.Description,
                    AssetType = request.AssetType
                },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EmployeeAssetDTO>> GetAssetsAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();

            return await connection.QueryAsync<EmployeeAssetDTO>(
                "sp_GetEmployeeAssets",
                new { EmployeeID = employeeId },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<EmployeeProfileDTO?> GetProfileAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<EmployeeProfileDTO>(
                "sp_GetEmployeeProfile",
                new { EmployeeID = employeeId },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<SubmitFeedbackResponseDTO?> SubmitFeedbackAsync(Guid ticketId, Guid employeeId, SubmitFeedbackRequestDTO request)
        {
            using var connection = _dbConnection.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<SubmitFeedbackResponseDTO>(
                "sp_SubmitTicketFeedback",
                new
                {
                    TicketID = ticketId,
                    EmployeeID = employeeId,
                    Rating = request.Rating,
                    Comments = request.Comments
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
