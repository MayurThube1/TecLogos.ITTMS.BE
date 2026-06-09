namespace TecLogos.ITTMS.Models.Entities
{
    /// <summary>
    /// Maps to the [Authentication] table in TeclogosITTMS database.
    /// Stores login credentials and session tracking per employee.
    /// </summary>
    public class Authentication
    {
        public Guid ID { get; set; }
        public Guid EmployeeID { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

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
