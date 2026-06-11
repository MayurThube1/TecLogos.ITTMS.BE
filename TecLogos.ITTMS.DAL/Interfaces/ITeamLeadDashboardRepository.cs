using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.DAL.Interfaces
{
    /// <summary>
    /// Data access contract for Team Lead dashboard operations.
    /// </summary>
    public interface ITeamLeadDashboardRepository
    {
        /// <summary>
        /// Calls sp_GetTeamLeadDashboardSummary.
        /// </summary>
        Task<TeamLeadDashboardSummaryDTO?> GetDashboardSummaryAsync();

        /// <summary>
        /// Calls sp_GetAllTickets. Returns paged result (count + rows).
        /// </summary>
        Task<PagedResultDTO<TeamLeadTicketListDTO>> GetAllTicketsAsync(string? status, string? priority, Guid? assignedTo, int pageNumber, int pageSize);

        /// <summary>
        /// Calls sp_AssignTicket. Assigns ticket and returns true if successful.
        /// </summary>
        Task<bool> AssignTicketAsync(Guid ticketId, Guid assignedToId, Guid assignedById);

        /// <summary>
        /// Calls sp_EscalateTicket. Escalates ticket priority to Critical and returns true if successful.
        /// </summary>
        Task<bool> EscalateTicketAsync(Guid ticketId, Guid escalatedById);

        /// <summary>
        /// Calls sp_GetEngineerWorkload.
        /// </summary>
        Task<IEnumerable<EngineerWorkloadDTO>> GetEngineerWorkloadAsync();

        /// <summary>
        /// Calls sp_GetSlaOverview.
        /// </summary>
        Task<IEnumerable<SlaOverviewDTO>> GetSlaOverviewAsync();

        /// <summary>
        /// Fetches engineers list using inline query.
        /// </summary>
        Task<IEnumerable<EngineerLookupDTO>> GetEngineersLookupAsync();
    }
}
