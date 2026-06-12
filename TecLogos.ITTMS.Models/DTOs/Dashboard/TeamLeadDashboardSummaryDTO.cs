namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Summary counts for the Team Lead dashboard.
    /// Maps to sp_GetTeamLeadDashboardSummary output.
    /// </summary>
    public class TeamLeadDashboardSummaryDTO
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedToday { get; set; }
        public int ClosedTickets { get; set; }
        public int SLABreachedCount { get; set; }
        public int CriticalOpen { get; set; }
    }
}
