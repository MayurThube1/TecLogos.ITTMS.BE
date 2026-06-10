namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Full ticket detail returned from sp_GetEmployeeTicketDetail.
    /// Combines ticket header with related comments and attachments.
    /// </summary>
    public class EmployeeTicketDetailDTO
    {
        // Ticket header fields
        public Guid ID { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AssetType { get; set; }
        public DateTime Created { get; set; }
        public DateTime? ResolutionDueDate { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsSLABreached { get; set; }
        public string? AssignedToName { get; set; }
        public int? FeedbackRating { get; set; }
        public string? FeedbackComments { get; set; }

        // Related data
        public List<TicketCommentDTO> Comments { get; set; } = new();
        public List<TicketAttachmentDTO> Attachments { get; set; } = new();
    }
}
