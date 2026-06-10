using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Request payload for raising a new ticket via sp_RaiseTicket.
    /// </summary>
    public class RaiseTicketRequestDTO
    {
        [Required(ErrorMessage = "Category is required.")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SubCategory { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        [StringLength(50)]
        public string Priority { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(250)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AssetType { get; set; }
    }
}
