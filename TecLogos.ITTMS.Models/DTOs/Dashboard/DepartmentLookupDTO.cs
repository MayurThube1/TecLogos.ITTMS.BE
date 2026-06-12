using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Lookup DTO for department dropdown filters.
    /// Maps to sp_GetDepartmentList.
    /// </summary>
    public class DepartmentLookupDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
