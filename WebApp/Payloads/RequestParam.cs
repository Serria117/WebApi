using WebApp.Enums;
using WebApp.Services.CommonService;

namespace WebApp.Payloads;

public class RequestParam
{
    public int? Page { get; set; } = 1;
    public int? Size { get; set; } = 10;
    public string? SortBy { get; set; } = "Id";
    public string? OrderBy { get; set; } = SortOrder.DESC;
    public string? Keyword { get; set; }

    public string? From { get; set; }
    public string? To { get; set; }

    public string[] Fields { get; set; } = [];

    public RequestParam Valid()
    {
        if (Size is <= 0 or > 1000 or null)
        {
            Size = 10;
        }

        if (Page is < 1 or > 1000 or null)
        {
            Page = 1;
        }
        
        if(Fields.Contains("Password")) //Remove the password field from the request
        {
            Fields = Fields.Where(x => x != "Password").ToArray();
        }
        Keyword?.RemoveSpace();
        return this;
    }
}