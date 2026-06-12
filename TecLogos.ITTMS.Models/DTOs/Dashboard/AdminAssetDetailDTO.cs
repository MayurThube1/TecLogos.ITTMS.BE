using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Detailed asset information for Admin view.
    /// Maps to sp_GetAssetDetail.
    /// </summary>
    public class AdminAssetDetailDTO
    {
        public Guid Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string? AssetType { get; set; }
        public string? SerialNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? Status { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string? Location { get; set; }
        public string? AllocatedTo { get; set; }
        public DateTime? AllocatedDate { get; set; }
    }
}
