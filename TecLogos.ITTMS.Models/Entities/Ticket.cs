using System;

namespace TecLogos.ITTMS.Models.Entities
{
    /// <summary>
    /// Maps to the [Ticket_Request] table in TeclogosITTMS database.
    /// </summary>
    public class Ticket
    {
        public Guid Id { get; set; }
        public string? Number { get; set; }
        public string? RequestType { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? ContactNumber { get; set; }
        public string? EmailId { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? Priority { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? AssetType { get; set; }
        public string? Status { get; set; }
        public Guid? AssignedTo { get; set; }

        // Audit columns
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedByID { get; set; }
        public DateTime? Modified { get; set; }
        public Guid? ModifiedByID { get; set; }
        public DateTime? Deleted { get; set; }
        public Guid? DeletedByID { get; set; }
    }
}
