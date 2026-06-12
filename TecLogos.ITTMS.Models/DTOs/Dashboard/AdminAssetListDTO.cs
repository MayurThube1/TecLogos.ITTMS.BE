using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// DTO for the asset list row in the Admin dashboard.
    /// Maps to sp_GetAllAssets.
    /// </summary>
    public class AdminAssetListDTO
    {
        public Guid Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string? AssetType { get; set; }
        public string? SerialNumber { get; set; }
        public string? Status { get; set; }
        public string? AllocatedTo { get; set; }
        public DateTime? AllocatedDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
    }
}
