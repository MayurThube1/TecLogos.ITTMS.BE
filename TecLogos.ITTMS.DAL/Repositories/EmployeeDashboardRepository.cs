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
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);

            return await connection.QueryFirstOrDefaultAsync<EmployeeDashboardSummaryDTO>(
                "sp_GetEmployeeDashboardSummary",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<EmployeeTicketListDTO>> GetTicketsAsync(Guid employeeId, string? status, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);
            parameters.Add("@Status", status);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetEmployeeTickets",
                parameters,
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
            var parameters = new DynamicParameters();
            parameters.Add("@TicketID", ticketId);
            parameters.Add("@EmployeeID", employeeId);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetEmployeeTicketDetail",
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
        public async Task<RaiseTicketResponseDTO?> RaiseTicketAsync(Guid employeeId, RaiseTicketRequestDTO request)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);
            parameters.Add("@Category", request.Category);
            parameters.Add("@SubCategory", request.SubCategory);
            parameters.Add("@Priority", request.Priority);
            parameters.Add("@Subject", request.Subject);
            parameters.Add("@Description", request.Description);
            parameters.Add("@AssetType", request.AssetType);

            return await connection.QueryFirstOrDefaultAsync<RaiseTicketResponseDTO>(
                "sp_RaiseTicket",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EmployeeAssetDTO>> GetAssetsAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);

            return await connection.QueryAsync<EmployeeAssetDTO>(
                "sp_GetEmployeeAssets",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<EmployeeProfileDTO?> GetProfileAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);

            return await connection.QueryFirstOrDefaultAsync<EmployeeProfileDTO>(
                "sp_GetEmployeeProfile",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<SubmitFeedbackResponseDTO?> SubmitFeedbackAsync(Guid ticketId, Guid employeeId, SubmitFeedbackRequestDTO request)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TicketID", ticketId);
            parameters.Add("@EmployeeID", employeeId);
            parameters.Add("@Rating", request.Rating);
            parameters.Add("@Comments", request.Comments);

            return await connection.QueryFirstOrDefaultAsync<SubmitFeedbackResponseDTO>(
                "sp_SubmitTicketFeedback",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
