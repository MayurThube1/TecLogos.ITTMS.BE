using System;

namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// DTO representing workload statistics for a support engineer.
    /// Maps to sp_GetEngineerWorkload.
    /// </summary>
    public class EngineerWorkloadDTO
    {
        public Guid EngineerID { get; set; }
        public string? EngineerName { get; set; }
        public int OpenCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
        public int TotalAssignedCount { get; set; }
        public int SlaBreachedCount { get; set; }
    }
}
