using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// DTO for the asset request list row in the Admin dashboard.
    /// Maps to sp_GetAssetRequests.
    /// </summary>
    public class AdminAssetRequestListDTO
    {
        public Guid Id { get; set; }
        public string? EmployeeName { get; set; }
        public string? Department { get; set; }
        public string? AssetType { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime RequestedOn { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedOn { get; set; }
    }
}
