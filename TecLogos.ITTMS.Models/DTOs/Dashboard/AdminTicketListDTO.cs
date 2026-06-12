using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// DTO for the ticket list row in the Admin dashboard.
    /// Maps to sp_GetAllTicketsAdmin.
    /// </summary>
    public class AdminTicketListDTO
    {
        public Guid Id { get; set; }
        public string? Number { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? Priority { get; set; }
        public string? Subject { get; set; }
        public string? Status { get; set; }
        public DateTime RaisedOn { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? EmployeeName { get; set; }
        public string? Department { get; set; }
        public string? AssignedToName { get; set; }
        public bool IsSLABreached { get; set; }
        public DateTime? ResolutionDueDate { get; set; }
    }
}
