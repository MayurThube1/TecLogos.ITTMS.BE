using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Response DTO containing appointment and site visit details.
    /// </summary>
    public class AppointmentDetailsDTO
    {
        public Guid ID { get; set; }
        public Guid TicketID { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public int DurationMinutes { get; set; }
        public string AppointmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public Guid ScheduledByID { get; set; }
        public string ScheduledByName { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
