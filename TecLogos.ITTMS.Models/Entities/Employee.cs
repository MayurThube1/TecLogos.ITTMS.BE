namespace TecLogos.ITTMS.Models.Entities
{
    /// <summary>
    /// Maps to the [Employees] table in TeclogosITTMS database.
    /// </summary>
    public class Employee
    {
        public Guid ID { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public DateTime? JoiningDate { get; set; }
        public Guid? DepartmentID { get; set; }
        public Guid? DesignationID { get; set; }
        public Guid? ManagerID { get; set; }
        public string? MobileNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? GenderID { get; set; }

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

        /// <summary>
        /// Computed full name from FirstName, MiddleName, LastName.
        /// </summary>
        public string FullName =>
            string.Join(" ", new[] { FirstName, MiddleName, LastName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
