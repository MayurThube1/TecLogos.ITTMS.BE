namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Summary counts for the Admin dashboard.
    /// Maps to sp_GetAdminDashboardSummary output.
    /// </summary>
    public class AdminDashboardSummaryDTO
    {
        public int TotalEmployees { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int TotalAssets { get; set; }
        public int PendingAssetRequests { get; set; }
        public int SLABreachedCount { get; set; }
    }
}
