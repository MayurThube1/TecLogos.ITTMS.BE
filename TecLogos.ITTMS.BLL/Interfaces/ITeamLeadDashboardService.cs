using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for Team Lead dashboard operations.
    /// </summary>
    public interface ITeamLeadDashboardService
    {
        Task<TeamLeadDashboardSummaryDTO?> GetDashboardSummaryAsync();
        Task<PagedResultDTO<TeamLeadTicketListDTO>> GetAllTicketsAsync(string? status, string? priority, Guid? assignedTo, int pageNumber, int pageSize);
        Task<bool> AssignTicketAsync(Guid ticketId, Guid assignedToId, Guid assignedById);
        Task<bool> EscalateTicketAsync(Guid ticketId, Guid escalatedById);
        Task<IEnumerable<EngineerWorkloadDTO>> GetEngineerWorkloadAsync();
        Task<IEnumerable<SlaOverviewDTO>> GetSlaOverviewAsync();
        Task<IEnumerable<EngineerLookupDTO>> GetEngineersLookupAsync();
        Task<EmployeeTicketDetailDTO?> GetTicketDetailAsync(Guid ticketId);
    }
}
