using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Detailed employee information for Admin view.
    /// Maps to sp_GetEmployeeByIdAdmin.
    /// </summary>
    public class AdminEmployeeDetailDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Role { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int TotalTicketsRaised { get; set; }
        public int OpenTickets { get; set; }
        public int AssetsAllocated { get; set; }
    }
}
