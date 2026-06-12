using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Lookup DTO for role dropdown filters.
    /// Maps to sp_GetRoleList.
    /// </summary>
    public class RoleLookupDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
