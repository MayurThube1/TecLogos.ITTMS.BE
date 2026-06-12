using System;
using System.Threading.Tasks;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Interfaces
{
    public interface IITSupportEngineerService
    {
        Task<PagedResultDTO<AssignedTicketDTO>> GetAssignedTicketsAsync(Guid engineerId, string? status, int pageNumber, int pageSize);
        Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, string? remarks, Guid engineerId);
        Task<bool> AddWorkNoteAsync(Guid ticketId, string noteText, Guid engineerId);
        Task<bool> AddResolutionAsync(Guid ticketId, string resolutionText, Guid engineerId);
        Task<bool> EscalateTicketAsync(Guid ticketId, string reason, Guid engineerId);
        Task<bool> CloseTicketAsync(Guid ticketId, Guid engineerId);
        Task<bool> ScheduleAppointmentAsync(Guid ticketId, DateTime appointmentTime, string description, Guid engineerId);
        Task<bool> AllocateAssetAsync(Guid assetId, Guid employeeId, string? remarks, Guid engineerId);
        Task<bool> ReturnAssetAsync(Guid assetId, string? remarks, Guid engineerId);
        Task<SlaComplianceDTO> GetSlaComplianceAsync(Guid engineerId);
    }
}
