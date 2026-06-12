using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Lookup item DTO for engineers.
    /// Used in drop-down menus for ticket assignment.
    /// </summary>
    public class EngineerLookupDTO
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
    }
}
