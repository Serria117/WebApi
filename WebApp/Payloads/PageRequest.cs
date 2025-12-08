using WebApp.Enums;

namespace WebApp.Payloads;

public class PageRequest
{
    public string SortBy { get; set; } = "Id";
    public string OrderBy { get; set; } = SortOrder.DESC;
    public int Page { get; set; }
    public int Size { get; set; }
    public string Sort { get; set; } = "Id DESC";
    public string? Keyword { get; set; }
    public int? Total { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    
    public string[] Fields { get; set; } = [];

    /// <summary>
    /// Get paging and sorting params from request parameters.
    /// </summary>
    /// <param name="pr"></param>
    /// <returns>The page request object with the parameters extracted and validated from the request.</returns>
    public static PageRequest FromParams(RequestParam pr)
    {
        pr.Valid();
        return new PageRequest
        {
            Page = pr.Page ?? 1,
            Size = pr.Size ?? 1000,
            SortBy = pr.SortBy ?? "Id",
            OrderBy = pr.OrderBy ?? SortOrder.ASC,
            Sort = $"{pr.SortBy} {pr.OrderBy}",
            Keyword = pr.Keyword,
            From = pr.From,
            To = pr.To,
            Fields = pr.Fields
        };
    }
}