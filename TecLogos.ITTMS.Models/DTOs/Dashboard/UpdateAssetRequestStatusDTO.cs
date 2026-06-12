using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Request DTO for updating an asset request's status (Approve/Reject) by Admin.
    /// </summary>
    public class UpdateAssetRequestStatusDTO
    {
        [Required(ErrorMessage = "Status is required (Approved or Rejected).")]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}
