using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Request DTO for updating a ticket's status by Admin.
    /// </summary>
    public class UpdateTicketStatusRequestDTO
    {
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}
