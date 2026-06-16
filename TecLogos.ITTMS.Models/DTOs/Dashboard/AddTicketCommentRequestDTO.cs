using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Request DTO for adding comments, work notes, or resolution details to a ticket.
    /// </summary>
    public class AddTicketCommentRequestDTO
    {
        /// <summary>
        /// The type of the comment: 'WorkNote', 'UserComment', or 'Resolution'.
        /// </summary>
        [Required]
        [RegularExpression("^(WorkNote|UserComment|Resolution)$", ErrorMessage = "CommentType must be 'WorkNote', 'UserComment', or 'Resolution'.")]
        public string CommentType { get; set; } = "WorkNote";

        /// <summary>
        /// The content text of the comment.
        /// </summary>
        [Required]
        [StringLength(8000, MinimumLength = 1, ErrorMessage = "CommentText must be between 1 and 8000 characters.")]
        public string CommentText { get; set; } = string.Empty;
    }
}
