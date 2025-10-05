using System.Collections;
using Microsoft.IdentityModel.Tokens;
using WebApp.Mongo.DeserializedModel;
using X.PagedList;

namespace WebApp.Payloads;

/// <summary>
/// Basic response wrapper object
/// </summary>
public class ResponseBase
{
    public string? Code { get; set; }
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public long? PageNumber { get; set; }
    public long? PageSize { get; set; }
    public long? PageCount { get; set; }
    public long? TotalCount { get; set; }
    public object? Data { get; set; }

    public static ResponseBase Ok()
    {
        return new ResponseBase { Code = "200", Success = true };
    }

    public static ResponseBase Ok(string message)
    {
        return new ResponseBase { Code = "200", Success = true, Message = message };
    }

    public static ResponseBase Ok(string code, string message)
    {
        return new ResponseBase { Success = true, Message = message, Code = code };
    }
    public static ResponseBase OkResult(object data)
    {
        ResponseBase response = new()
        {
            Code = "200",
            Message = "OK",
            Data = data,
            TotalCount = 1
        };
        switch (data)
        {
            case IList list:
                response.TotalCount = list.Count;
                break;
            case IPagedList<object> pagedList:
                response.TotalCount = pagedList.TotalItemCount;
                response.PageNumber = pagedList.PageNumber;
                response.PageSize = pagedList.PageSize;
                response.PageCount = pagedList.PageCount;
                break;
            case IPaginatedDocResult paginatedDocResult:
                response.TotalCount = paginatedDocResult.Total;
                response.PageCount = paginatedDocResult.PageCount;
                response.PageNumber = paginatedDocResult.Page;
                response.PageCount = paginatedDocResult.PageCount;
                break;
        }

        return response;
    }

    public static ResponseBase Error(string mesage, params string[] details)
    {
        return new ResponseBase
        {
            Success = false,
            Message = mesage,
            Data = !details.IsNullOrEmpty() ? details.ToList() : null,
            Code = "99",
        };
    }

    public static ResponseBase Error(string mesage, List<string> details)
    {
        return new ResponseBase
        {
            Success = false,
            Message = mesage,
            Data = details
        };
    }

    public static ResponseBase Error400(string message, params string[] details)
    {
        return new ResponseBase
        {
            Code = "400",
            Success = false,
            Message = message,
            Data = details
        };
    }
    
    public static ResponseBase Error404(string message, params string[] details)
    {
        return new ResponseBase
        {
            Code = "404",
            Success = false,
            Message = message,
            Data = details
        };
    }

    public static ResponseBase Error500(string message, params string[] details)
    {
        return new ResponseBase
        {
            Code = "500",
            Success = false,
            Message = message,
            Data = details
        };
    }

    public int ToHttpStatusCode()
    {
        return Code switch
        {
            "200" => 200,
            "400" => 400,
            "404" => 404,
            _ => 500
        };
    }
}