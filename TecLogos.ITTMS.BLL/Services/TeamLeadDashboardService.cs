using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Services
{
    /// <summary>
    /// Business logic service implementing Team Lead dashboard operations.
    /// </summary>
    public class TeamLeadDashboardService : ITeamLeadDashboardService
    {
        private readonly ITeamLeadDashboardRepository _repository;

        public TeamLeadDashboardService(ITeamLeadDashboardRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<TeamLeadDashboardSummaryDTO?> GetDashboardSummaryAsync()
        {
            return await _repository.GetDashboardSummaryAsync();
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<TeamLeadTicketListDTO>> GetAllTicketsAsync(
            string? status, string? priority, Guid? assignedTo, int pageNumber, int pageSize)
        {
            return await _repository.GetAllTicketsAsync(status, priority, assignedTo, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<bool> AssignTicketAsync(Guid ticketId, Guid assignedToId, Guid assignedById)
        {
            return await _repository.AssignTicketAsync(ticketId, assignedToId, assignedById);
        }

        /// <inheritdoc />
        public async Task<bool> EscalateTicketAsync(Guid ticketId, Guid escalatedById)
        {
            return await _repository.EscalateTicketAsync(ticketId, escalatedById);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EngineerWorkloadDTO>> GetEngineerWorkloadAsync()
        {
            return await _repository.GetEngineerWorkloadAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SlaOverviewDTO>> GetSlaOverviewAsync()
        {
            return await _repository.GetSlaOverviewAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EngineerLookupDTO>> GetEngineersLookupAsync()
        {
            return await _repository.GetEngineersLookupAsync();
        }

        /// <inheritdoc />
        public async Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId)
        {
            return await _repository.GetTicketDetailAsync(ticketId);
        }
    }
}
