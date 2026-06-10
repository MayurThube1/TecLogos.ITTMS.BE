namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Summary counts for the employee dashboard.
    /// Maps to sp_GetEmployeeDashboardSummary output.
    /// </summary>
    public class EmployeeDashboardSummaryDTO
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int AssignedAssets { get; set; }
    }
}
