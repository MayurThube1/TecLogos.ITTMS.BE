using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Services
{
    /// <summary>
    /// Thin orchestration layer for employee dashboard operations.
    /// Delegates directly to the repository; no additional business logic
    /// beyond what the stored procedures already enforce.
    /// </summary>
    public class EmployeeDashboardService : IEmployeeDashboardService
    {
        private readonly IEmployeeDashboardRepository _repository;

        public EmployeeDashboardService(IEmployeeDashboardRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<EmployeeDashboardSummaryDTO?> GetDashboardSummaryAsync(Guid employeeId)
        {
            return await _repository.GetDashboardSummaryAsync(employeeId);
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<EmployeeTicketListDTO>> GetTicketsAsync(Guid employeeId, string? status, int pageNumber, int pageSize)
        {
            return await _repository.GetTicketsAsync(employeeId, status, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId, Guid employeeId)
        {
            return await _repository.GetTicketDetailAsync(ticketId, employeeId);
        }

        /// <inheritdoc />
        public async Task<RaiseTicketResponseDTO?> RaiseTicketAsync(Guid employeeId, RaiseTicketRequestDTO request)
        {
            return await _repository.RaiseTicketAsync(employeeId, request);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EmployeeAssetDTO>> GetAssetsAsync(Guid employeeId)
        {
            return await _repository.GetAssetsAsync(employeeId);
        }

        /// <inheritdoc />
        public async Task<EmployeeProfileDTO?> GetProfileAsync(Guid employeeId)
        {
            return await _repository.GetProfileAsync(employeeId);
        }

        /// <inheritdoc />
        public async Task<SubmitFeedbackResponseDTO?> SubmitFeedbackAsync(Guid ticketId, Guid employeeId, SubmitFeedbackRequestDTO request)
        {
            return await _repository.SubmitFeedbackAsync(ticketId, employeeId, request);
        }
    }
}
