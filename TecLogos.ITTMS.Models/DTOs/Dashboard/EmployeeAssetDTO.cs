namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Asset allocated to an employee, returned from sp_GetEmployeeAssets.
    /// </summary>
    public class EmployeeAssetDTO
    {
        public Guid ID { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string AllocationStatus { get; set; } = string.Empty;
        public DateTime? AllocatedDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public int WarrantyDaysRemaining { get; set; }
        public bool IsWarrantyExpired { get; set; }
    }
}
