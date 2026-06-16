using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Interfaces
{
    /// <summary>
    /// Service contract for IT Support Engineer operations matching the user's controller.
    /// </summary>
    public interface IITSupportEngineerService
    {
        Task<PagedResultDTO<AssignedTicketDTO>> GetAssignedTicketsAsync(Guid engineerId, string? status, int pageNumber, int pageSize);
        Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, string? remarks, Guid updatedById);
        Task<bool> AddWorkNoteAsync(Guid ticketId, string noteText, Guid commentedById);
        Task<bool> AddResolutionAsync(Guid ticketId, string resolutionText, Guid commentedById);
        Task<bool> EscalateTicketAsync(Guid ticketId, string reason, Guid escalatedById);
        Task<bool> CloseTicketAsync(Guid ticketId, Guid closedById);
        Task<bool> ScheduleAppointmentAsync(Guid ticketId, DateTime appointmentTime, string description, Guid engineerId);
        Task<IEnumerable<AppointmentDetailsDTO>> GetTicketAppointmentsAsync(Guid ticketId);
        Task<bool> AllocateAssetAsync(Guid assetId, Guid employeeId, string? remarks, Guid allocatedById);
        Task<bool> ReturnAssetAsync(Guid assetId, string? remarks, Guid returnedById);
        Task<SlaComplianceDTO> GetSlaComplianceAsync(Guid engineerId);
    }
}
