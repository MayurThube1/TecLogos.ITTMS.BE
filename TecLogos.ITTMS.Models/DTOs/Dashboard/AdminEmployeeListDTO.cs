using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// DTO for the employee list row in the Admin dashboard.
    /// Maps to sp_GetAllEmployees.
    /// </summary>
    public class AdminEmployeeListDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? JoiningDate { get; set; }
    }
}
