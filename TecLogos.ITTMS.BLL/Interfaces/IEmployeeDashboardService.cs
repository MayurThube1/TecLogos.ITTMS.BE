using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for employee dashboard operations.
    /// </summary>
    public interface IEmployeeDashboardService
    {
        Task<EmployeeDashboardSummaryDTO?> GetDashboardSummaryAsync(Guid employeeId);
        Task<PagedResultDTO<EmployeeTicketListDTO>> GetTicketsAsync(Guid employeeId, string? status, int pageNumber, int pageSize);
        Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId, Guid employeeId);
        Task<RaiseTicketResponseDTO?> RaiseTicketAsync(Guid employeeId, RaiseTicketRequestDTO request);
        Task<IEnumerable<EmployeeAssetDTO>> GetAssetsAsync(Guid employeeId);
        Task<EmployeeProfileDTO?> GetProfileAsync(Guid employeeId);
        Task<SubmitFeedbackResponseDTO?> SubmitFeedbackAsync(Guid ticketId, Guid employeeId, SubmitFeedbackRequestDTO request);
    }
}
