using System;
using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Unified request DTO for scheduling a new appointment or updating status of an existing appointment.
    /// </summary>
    public class ManageAppointmentDTO
    {
        // For scheduling (Create)
        public Guid? TicketId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public string? AppointmentType { get; set; } // 'SiteVisit' | 'RemoteSupport'
        public string? Remarks { get; set; }

        // For status updates (Update)
        public Guid? AppointmentId { get; set; }
        public string? Status { get; set; } // 'Scheduled' | 'Completed' | 'Cancelled'
    }
}
