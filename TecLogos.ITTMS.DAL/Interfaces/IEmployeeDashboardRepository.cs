using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.DAL.Interfaces
{
    /// <summary>
    /// Data access contract for employee dashboard operations.
    /// One method per stored procedure.
    /// </summary>
    public interface IEmployeeDashboardRepository
    {
        /// <summary>
        /// Calls sp_GetEmployeeDashboardSummary.
        /// </summary>
        Task<EmployeeDashboardSummaryDTO?> GetDashboardSummaryAsync(Guid employeeId);

        /// <summary>
        /// Calls sp_GetEmployeeTickets. Returns paged result (count + rows).
        /// </summary>
        Task<PagedResultDTO<EmployeeTicketListDTO>> GetTicketsAsync(Guid employeeId, string? status, int pageNumber, int pageSize);

        /// <summary>
        /// Calls sp_GetEmployeeTicketDetail. Returns ticket header + comments + attachments.
        /// </summary>
        Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId, Guid employeeId);

        /// <summary>
        /// Calls sp_RaiseTicket. Returns new ticket Id, Number, Status.
        /// </summary>
        Task<RaiseTicketResponseDTO?> RaiseTicketAsync(Guid employeeId, RaiseTicketRequestDTO request);

        /// <summary>
        /// Calls sp_GetEmployeeAssets.
        /// </summary>
        Task<IEnumerable<EmployeeAssetDTO>> GetAssetsAsync(Guid employeeId);

        /// <summary>
        /// Calls sp_GetEmployeeProfile.
        /// </summary>
        Task<EmployeeProfileDTO?> GetProfileAsync(Guid employeeId);

        /// <summary>
        /// Calls sp_SubmitTicketFeedback.
        /// </summary>
        Task<SubmitFeedbackResponseDTO?> SubmitFeedbackAsync(Guid ticketId, Guid employeeId, SubmitFeedbackRequestDTO request);
    }
}
