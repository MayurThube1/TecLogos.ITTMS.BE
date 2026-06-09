namespace TecLogos.ITTMS.Models.Entities
{
    /// <summary>
    /// Maps to the [Role] table in TeclogosITTMS database.
    /// </summary>
    public class Role
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

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
