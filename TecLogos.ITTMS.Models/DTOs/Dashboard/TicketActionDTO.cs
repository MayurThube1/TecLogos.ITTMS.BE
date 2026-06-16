using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Unified request DTO representing a ticket action (UpdateStatus, AddComment, Escalate, Close).
    /// </summary>
    public class TicketActionDTO
    {
        /// <summary>
        /// The action to perform. Valid values: 'UpdateStatus', 'AddComment', 'Escalate', 'Close'.
        /// </summary>
        [Required]
        [RegularExpression("^(UpdateStatus|AddComment|Escalate|Close)$", ErrorMessage = "Action must be 'UpdateStatus', 'AddComment', 'Escalate', or 'Close'.")]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// The new status. Required only when Action is 'UpdateStatus'.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// The type of the comment: 'WorkNote', 'UserComment', or 'Resolution'. Required only when Action is 'AddComment'.
        /// </summary>
        public string? CommentType { get; set; }

        /// <summary>
        /// The comment body content. Required only when Action is 'AddComment'.
        /// </summary>
        public string? CommentText { get; set; }

        /// <summary>
        /// Remarks for status updates or escalation details. Required only when Action is 'Escalate'. Optional for 'UpdateStatus'.
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// Resolution details. Optional when Action is 'Close'.
        /// </summary>
        public string? ResolutionDetails { get; set; }
    }
}
