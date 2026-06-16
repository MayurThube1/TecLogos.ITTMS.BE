using System;
using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Request DTO for scheduling ticket appointments or site visits.
    /// </summary>
    public class ScheduleAppointmentRequestDTO
    {
        /// <summary>
        /// The date and time of the appointment.
        /// </summary>
        [Required]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// Expected duration of the appointment in minutes.
        /// </summary>
        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
        public int DurationMinutes { get; set; } = 60;

        /// <summary>
        /// The type of appointment: 'SiteVisit' or 'RemoteSupport'.
        /// </summary>
        [Required]
        [RegularExpression("^(SiteVisit|RemoteSupport)$", ErrorMessage = "AppointmentType must be 'SiteVisit' or 'RemoteSupport'.")]
        public string AppointmentType { get; set; } = "SiteVisit";

        /// <summary>
        /// Optional remarks or descriptions.
        /// </summary>
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}
