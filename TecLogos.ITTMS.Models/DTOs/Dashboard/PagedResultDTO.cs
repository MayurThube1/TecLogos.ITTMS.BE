namespace TecLogos.ITTMS.Models.DTOs.Dashboard
{
    /// <summary>
    /// Generic paged result wrapper.
    /// </summary>
    public class PagedResultDTO<T>
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
