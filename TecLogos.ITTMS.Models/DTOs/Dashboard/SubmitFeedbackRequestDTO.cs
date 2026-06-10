using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Request payload for submitting ticket feedback via sp_SubmitTicketFeedback.
    /// </summary>
    public class SubmitFeedbackRequestDTO
    {
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Response returned from sp_SubmitTicketFeedback.
    /// </summary>
    public class SubmitFeedbackResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response returned from sp_RaiseTicket with new ticket info.
    /// </summary>
    public class RaiseTicketResponseDTO
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
