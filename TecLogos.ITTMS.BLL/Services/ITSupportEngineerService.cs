using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Services
{
    public class ITSupportEngineerService : IITSupportEngineerService
    {
        private readonly IITSupportEngineerRepository _repository;
        private readonly ILogger<ITSupportEngineerService> _logger;

        public ITSupportEngineerService(IITSupportEngineerRepository repository, ILogger<ITSupportEngineerService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PagedResultDTO<AssignedTicketDTO>> GetAssignedTicketsAsync(Guid engineerId, string? status, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Getting assigned tickets for EngineerId: {EngineerId}, Status: {Status}, Page: {Page}", engineerId, status, pageNumber);
            
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            return await _repository.GetAssignedTicketsAsync(engineerId, status, pageNumber, pageSize);
        }

        public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, string? remarks, Guid engineerId)
        {
            _logger.LogInformation("Updating ticket {TicketId} status to {Status} by Engineer: {EngineerId}", ticketId, status, engineerId);
            
            if (ticketId == Guid.Empty)
                throw new ArgumentException("TicketID cannot be empty.", nameof(ticketId));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty.", nameof(status));

            return await _repository.UpdateTicketStatusAsync(ticketId, status, remarks, engineerId);
        }

        public async Task<bool> AddWorkNoteAsync(Guid ticketId, string noteText, Guid engineerId)
        {
            _logger.LogInformation("Adding work note to ticket {TicketId} by Engineer: {EngineerId}", ticketId, engineerId);

            if (ticketId == Guid.Empty)
                throw new ArgumentException("TicketID cannot be empty.", nameof(ticketId));
            if (string.IsNullOrWhiteSpace(noteText))
                throw new ArgumentException("NoteText cannot be empty.", nameof(noteText));

            return await _repository.AddWorkNoteAsync(ticketId, noteText, engineerId);
        }

        public async Task<bool> AddResolutionAsync(Guid ticketId, string resolutionText, Guid engineerId)
        {
            _logger.LogInformation("Adding resolution for ticket {TicketId} by Engineer: {EngineerId}", ticketId, engineerId);

            if (ticketId == Guid.Empty)
                throw new ArgumentException("TicketID cannot be empty.", nameof(ticketId));
            if (string.IsNullOrWhiteSpace(resolutionText))
                throw new ArgumentException("ResolutionText cannot be empty.", nameof(resolutionText));

            return await _repository.AddResolutionAsync(ticketId, resolutionText, engineerId);
        }

        public async Task<bool> EscalateTicketAsync(Guid ticketId, string reason, Guid engineerId)
        {
            _logger.LogInformation("Escalating ticket {TicketId} by Engineer: {EngineerId}. Reason: {Reason}", ticketId, engineerId, reason);

            if (ticketId == Guid.Empty)
                throw new ArgumentException("TicketID cannot be empty.", nameof(ticketId));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason cannot be empty.", nameof(reason));

            return await _repository.EscalateTicketAsync(ticketId, reason, engineerId);
        }

        public async Task<bool> CloseTicketAsync(Guid ticketId, Guid engineerId)
        {
            _logger.LogInformation("Closing ticket {TicketId} by Engineer: {EngineerId}", ticketId, engineerId);

            if (ticketId == Guid.Empty)
                throw new ArgumentException("TicketID cannot be empty.", nameof(ticketId));

            return await _repository.CloseTicketAsync(ticketId, engineerId);
        }

        public async Task<bool> ScheduleAppointmentAsync(Guid ticketId, DateTime appointmentTime, string description, Guid engineerId)
        {
            _logger.LogInformation("Scheduling appointment for ticket {TicketId} by Engineer: {EngineerId} at {Time}", ticketId, engineerId, appointmentTime);

            if (ticketId == Guid.Empty)
                throw new ArgumentException("TicketID cannot be empty.", nameof(ticketId));
            if (appointmentTime == default)
                throw new ArgumentException("Invalid appointment time.", nameof(appointmentTime));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty.", nameof(description));

            return await _repository.ScheduleAppointmentAsync(ticketId, appointmentTime, description, engineerId);
        }

        public async Task<bool> AllocateAssetAsync(Guid assetId, Guid employeeId, string? remarks, Guid engineerId)
        {
            _logger.LogInformation("Allocating asset {AssetId} to employee {EmployeeId} by Engineer: {EngineerId}", assetId, employeeId, engineerId);

            if (assetId == Guid.Empty)
                throw new ArgumentException("AssetID cannot be empty.", nameof(assetId));
            if (employeeId == Guid.Empty)
                throw new ArgumentException("EmployeeID cannot be empty.", nameof(employeeId));

            return await _repository.AllocateAssetAsync(assetId, employeeId, remarks, engineerId);
        }

        public async Task<bool> ReturnAssetAsync(Guid assetId, string? remarks, Guid engineerId)
        {
            _logger.LogInformation("Returning asset {AssetId} by Engineer: {EngineerId}", assetId, engineerId);

            if (assetId == Guid.Empty)
                throw new ArgumentException("AssetID cannot be empty.", nameof(assetId));

            return await _repository.ReturnAssetAsync(assetId, remarks, engineerId);
        }

        public async Task<SlaComplianceDTO> GetSlaComplianceAsync(Guid engineerId)
        {
            _logger.LogInformation("Tracking SLA Compliance for Engineer: {EngineerId}", engineerId);

            if (engineerId == Guid.Empty)
                throw new ArgumentException("EngineerID cannot be empty.", nameof(engineerId));

            return await _repository.GetSlaComplianceAsync(engineerId);
        }
    }
}
