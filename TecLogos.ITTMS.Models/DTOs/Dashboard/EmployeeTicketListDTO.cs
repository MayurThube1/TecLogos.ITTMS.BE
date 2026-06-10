namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Ticket row returned from sp_GetEmployeeTickets (paged result set).
    /// </summary>
    public class EmployeeTicketListDTO
    {
        public Guid ID { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AssetType { get; set; }
        public DateTime Created { get; set; }
        public DateTime? ResolutionDueDate { get; set; }
        public bool IsSLABreached { get; set; }
        public string? AssignedToName { get; set; }
    }
}
