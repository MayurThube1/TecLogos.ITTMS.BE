namespace TecLogos.ITTMS.Models.Entities
{
    /// <summary>
    /// Maps to the [EmployeeRoles] junction table in TeclogosITTMS database.
    /// Links Employees to Roles (many-to-many).
    /// </summary>
    public class EmployeeRole
    {
        public Guid ID { get; set; }
        public Guid EmployeeID { get; set; }
        public Guid RoleID { get; set; }

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
