using System;
using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Request DTO for reassigning a ticket to a different engineer by Admin.
    /// </summary>
    public class ReassignTicketRequestDTO
    {
        [Required(ErrorMessage = "AssignedToID is required.")]
        public Guid AssignedToID { get; set; }
    }
}
