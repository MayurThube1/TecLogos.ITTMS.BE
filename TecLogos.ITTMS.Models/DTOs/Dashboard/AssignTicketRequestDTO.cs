using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Request DTO for assigning a ticket to an engineer.
    /// </summary>
    public class AssignTicketRequestDTO
    {
        public Guid AssignedToID { get; set; }
    }
}
