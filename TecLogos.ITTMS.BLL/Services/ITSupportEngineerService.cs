using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.BLL.Services
{
    /// <summary>
    /// Service implementation handling logic for IT Support Engineer features.
    /// Updated to match the user's controller and interface signatures.
    /// </summary>
    public class ITSupportEngineerService : IITSupportEngineerService
    {
        private readonly IITSupportEngineerRepository _repository;
        private static bool _tablesChecked = false;
        private static readonly object _lock = new object();

        public ITSupportEngineerService(IITSupportEngineerRepository repository)
        {
            _repository = repository;
        }

        private async Task EnsureTablesExistAsync()
        {
            if (!_tablesChecked)
            {
                await _repository.EnsureTablesCreatedAsync();
                lock (_lock)
                {
                    _tablesChecked = true;
                }
            }
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AssignedTicketDTO>> GetAssignedTicketsAsync(Guid engineerId, string? status, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 15;
            return await _repository.GetAssignedTicketsAsync(engineerId, status, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, Guid updatedById)
        {
            if (ticketId == Guid.Empty) throw new ArgumentException("Ticket ID cannot be empty.", nameof(ticketId));
            if (updatedById == Guid.Empty) throw new ArgumentException("Updated By ID cannot be empty.", nameof(updatedById));
            if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Status cannot be empty.", nameof(status));

            return await _repository.UpdateTicketStatusAsync(ticketId, status, updatedById);
        }

        /// <inheritdoc />
        public async Task<bool> AddWorkNoteAsync(Guid ticketId, string noteText, Guid commentedById)
        {
            if (ticketId == Guid.Empty) throw new ArgumentException("Ticket ID cannot be empty.", nameof(ticketId));
            if (commentedById == Guid.Empty) throw new ArgumentException("Commented By ID cannot be empty.", nameof(commentedById));
            if (string.IsNullOrWhiteSpace(noteText)) throw new ArgumentException("Note text cannot be empty.", nameof(noteText));

            return await _repository.AddWorkNoteAsync(ticketId, noteText, commentedById);
        }

        /// <inheritdoc />
        public async Task<bool> AddResolutionAsync(Guid ticketId, string resolutionText, Guid commentedById)
        {
            if (ticketId == Guid.Empty) throw new ArgumentException("Ticket ID cannot be empty.", nameof(ticketId));
            if (commentedById == Guid.Empty) throw new ArgumentException("Commented By ID cannot be empty.", nameof(commentedById));
            if (string.IsNullOrWhiteSpace(resolutionText)) throw new ArgumentException("Resolution text cannot be empty.", nameof(resolutionText));

            return await _repository.AddResolutionAsync(ticketId, resolutionText, commentedById);
        }

        /// <inheritdoc />
        public async Task<bool> EscalateTicketAsync(Guid ticketId, string reason, Guid escalatedById)
        {
            if (ticketId == Guid.Empty) throw new ArgumentException("Ticket ID cannot be empty.", nameof(ticketId));
            if (escalatedById == Guid.Empty) throw new ArgumentException("Escalated By ID cannot be empty.", nameof(escalatedById));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason cannot be empty.", nameof(reason));

            return await _repository.EscalateTicketAsync(ticketId, reason, escalatedById);
        }

        /// <inheritdoc />
        public async Task<bool> CloseTicketAsync(Guid ticketId, Guid closedById)
        {
            if (ticketId == Guid.Empty) throw new ArgumentException("Ticket ID cannot be empty.", nameof(ticketId));
            if (closedById == Guid.Empty) throw new ArgumentException("Closed By ID cannot be empty.", nameof(closedById));

            return await _repository.CloseTicketAsync(ticketId, closedById);
        }

        /// <inheritdoc />
        public async Task<bool> ScheduleAppointmentAsync(Guid ticketId, DateTime appointmentTime, string description, Guid engineerId)
        {
            if (ticketId == Guid.Empty) throw new ArgumentException("Ticket ID cannot be empty.", nameof(ticketId));
            if (engineerId == Guid.Empty) throw new ArgumentException("Engineer ID cannot be empty.", nameof(engineerId));
            if (appointmentTime < DateTime.UtcNow.AddDays(-1)) throw new ArgumentException("Appointment date cannot be in the past.");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty.");

            await EnsureTablesExistAsync();
            return await _repository.ScheduleAppointmentAsync(ticketId, appointmentTime, description, engineerId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AppointmentDetailsDTO>> GetTicketAppointmentsAsync(Guid ticketId)
        {
            if (ticketId == Guid.Empty) throw new ArgumentException("Ticket ID cannot be empty.", nameof(ticketId));

            await EnsureTablesExistAsync();
            return await _repository.GetTicketAppointmentsAsync(ticketId);
        }

        /// <inheritdoc />
        public async Task<bool> AllocateAssetAsync(Guid assetId, Guid employeeId, string? remarks, Guid allocatedById)
        {
            if (assetId == Guid.Empty) throw new ArgumentException("Asset ID cannot be empty.", nameof(assetId));
            if (employeeId == Guid.Empty) throw new ArgumentException("Employee ID cannot be empty.", nameof(employeeId));
            if (allocatedById == Guid.Empty) throw new ArgumentException("Allocated By ID cannot be empty.", nameof(allocatedById));

            return await _repository.AllocateAssetAsync(assetId, employeeId, remarks, allocatedById);
        }

        /// <inheritdoc />
        public async Task<bool> ReturnAssetAsync(Guid assetId, string? remarks, Guid returnedById)
        {
            if (assetId == Guid.Empty) throw new ArgumentException("Asset ID cannot be empty.", nameof(assetId));
            if (returnedById == Guid.Empty) throw new ArgumentException("Returned By ID cannot be empty.", nameof(returnedById));

            return await _repository.ReturnAssetAsync(assetId, remarks, returnedById);
        }

        /// <inheritdoc />
        public async Task<SlaComplianceDTO> GetSlaComplianceAsync(Guid engineerId)
        {
            if (engineerId == Guid.Empty) throw new ArgumentException("Engineer ID cannot be empty.", nameof(engineerId));
            return await _repository.GetSlaComplianceAsync(engineerId);
        }
    }
}
