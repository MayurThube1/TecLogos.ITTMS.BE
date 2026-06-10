namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Employee profile information returned from sp_GetEmployeeProfile.
    /// </summary>
    public class EmployeeProfileDTO
    {
        public Guid ID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Role { get; set; }
        public string? ManagerName { get; set; }
        public DateTime? JoiningDate { get; set; }
    }
}
