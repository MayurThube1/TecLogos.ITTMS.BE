using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// DTO for SLA tracking overview.
    /// Maps to sp_GetSlaOverview.
    /// </summary>
    public class SlaOverviewDTO
    {
        public Guid TicketId { get; set; }
        public string? TicketNumber { get; set; }
        public string? Subject { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? ResponseDueDate { get; set; }
        public DateTime? ResolutionDueDate { get; set; }
        public bool IsResponseBreached { get; set; }
        public bool IsResolutionBreached { get; set; }
        public int MinutesRemaining { get; set; }
    }
}
