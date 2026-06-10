namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Comment on a ticket, returned as part of ticket detail.
    /// </summary>
    public class TicketCommentDTO
    {
        public Guid ID { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public string CommentedByName { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; }
        public bool IsInternal { get; set; }
    }
}
