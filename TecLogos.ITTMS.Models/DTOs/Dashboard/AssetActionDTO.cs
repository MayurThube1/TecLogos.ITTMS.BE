using System;
using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Unified request DTO for asset actions (Allocate or Return).
    /// </summary>
    public class AssetActionDTO
    {
        /// <summary>
        /// The action to perform. Valid values: 'Allocate', 'Return'.
        /// </summary>
        [Required]
        [RegularExpression("^(Allocate|Return)$", ErrorMessage = "Action must be 'Allocate' or 'Return'.")]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the asset.
        /// </summary>
        [Required]
        public Guid AssetId { get; set; }

        /// <summary>
        /// The employee ID to allocate the asset to. Required only when Action is 'Allocate'.
        /// </summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>
        /// Optional associated asset request ID if allocating to satisfy a request.
        /// </summary>
        public Guid? AssetRequestId { get; set; }

        /// <summary>
        /// Remarks for the allocation or return transaction.
        /// </summary>
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}
