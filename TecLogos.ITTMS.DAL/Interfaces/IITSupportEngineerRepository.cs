using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.DAL.Interfaces
{
    /// <summary>
    /// Data access contract for IT Support Engineer operations matching the user's controller interface.
    /// Uses inline SQL/Dapper instead of stored procedures.
    /// </summary>
    public interface IITSupportEngineerRepository
    {
        /// <summary>
        /// Ensures all custom tables needed for IT support operations (like Ticket_Appointments) exist.
        /// </summary>
        Task EnsureTablesCreatedAsync();

        /// <summary>
        /// Retrieves tickets assigned to a specific engineer.
        /// </summary>
        Task<PagedResultDTO<AssignedTicketDTO>> GetAssignedTicketsAsync(Guid engineerId, string? status, int pageNumber, int pageSize);

        /// <summary>
        /// Updates the status of a ticket.
        /// </summary>
        Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, Guid updatedById);

        /// <summary>
        /// Adds a work note to a ticket.
        /// </summary>
        Task<bool> AddWorkNoteAsync(Guid ticketId, string noteText, Guid commentedById);

        /// <summary>
        /// Adds a resolution to a ticket.
        /// </summary>
        Task<bool> AddResolutionAsync(Guid ticketId, string resolutionText, Guid commentedById);

        /// <summary>
        /// Escalates the ticket's priority to Critical.
        /// </summary>
        Task<bool> EscalateTicketAsync(Guid ticketId, string reason, Guid escalatedById);

        /// <summary>
        /// Closes the ticket, sets resolved dates.
        /// </summary>
        Task<bool> CloseTicketAsync(Guid ticketId, Guid closedById);

        /// <summary>
        /// Schedules a new appointment or site visit for a ticket.
        /// </summary>
        Task<bool> ScheduleAppointmentAsync(Guid ticketId, DateTime appointmentTime, string description, Guid engineerId);

        /// <summary>
        /// Retrieves all appointments associated with a ticket.
        /// </summary>
        Task<IEnumerable<AppointmentDetailsDTO>> GetTicketAppointmentsAsync(Guid ticketId);

        /// <summary>
        /// Allocates a specific asset to an employee.
        /// </summary>
        Task<bool> AllocateAssetAsync(Guid assetId, Guid employeeId, string? remarks, Guid allocatedById);

        /// <summary>
        /// Records an asset return and updates asset status.
        /// </summary>
        Task<bool> ReturnAssetAsync(Guid assetId, string? remarks, Guid returnedById);

        /// <summary>
        /// Fetches SLA compliance stats for tickets assigned to an engineer.
        /// </summary>
        Task<SlaComplianceDTO> GetSlaComplianceAsync(Guid engineerId);
    }
}
