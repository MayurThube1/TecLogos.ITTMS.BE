namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Attachment on a ticket, returned as part of ticket detail.
    /// </summary>
    public class TicketAttachmentDTO
    {
        public Guid ID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
