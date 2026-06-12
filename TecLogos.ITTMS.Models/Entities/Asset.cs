using System;

namespace TecLogos.ITTMS.Models.Entities
{
    /// <summary>
    /// Maps to the [Asset_Master] table in TeclogosITTMS database.
    /// </summary>
    public class Asset
    {
        public Guid AssetId { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public string? AssetType { get; set; }
        public string? Brand { get; set; }
        public string? ModelNo { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string? VendorName { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public string? AssetStatus { get; set; }
        public string? Location { get; set; }
        public string? Department { get; set; }
        public Guid? AssignedToEmployeeId { get; set; }
        public string? Remarks { get; set; }

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
