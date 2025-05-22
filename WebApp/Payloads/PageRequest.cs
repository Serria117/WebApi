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
    /// Get paging and sorting params from request
    /// </summary>
    /// <param name="req"></param>
    /// <returns>The page request object with the parameters extracted and validated from the request.</returns>
    public static PageRequest BuildRequest(RequestParam req)
    {
        req.Valid();
        return new PageRequest
        {
            Page = req.Page ?? 1,
            Size = req.Size ?? 10,
            SortBy = req.SortBy ?? "Id",
            OrderBy = req.OrderBy ?? SortOrder.ASC,
            Sort = $"{req.SortBy} {req.OrderBy}",
            Keyword = req.Keyword,
            From = req.From,
            To = req.To,
            Fields = req.Fields
        };
    }
}