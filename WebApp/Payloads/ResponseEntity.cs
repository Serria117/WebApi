using System.Collections;
using Microsoft.IdentityModel.Tokens;
using WebApp.Mongo.DeserializedModel;
using X.PagedList;

namespace WebApp.Payloads;

/// <summary>
/// Basic response wrapper object
/// </summary>
public class ResponseEntity
{
    public string? Code { get; set; }
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public long? PageNumber { get; set; }
    public long? PageSize { get; set; }
    public long? PageCount { get; set; }
    public long? TotalCount { get; set; }
    public object? Data { get; set; }

    public static ResponseEntity Ok()
    {
        return new ResponseEntity { Code = "200", Success = true };
    }

    public static ResponseEntity Ok(string message)
    {
        return new ResponseEntity { Code = "200", Success = true, Message = message };
    }

    public static ResponseEntity Ok(string code, string message)
    {
        return new ResponseEntity { Success = true, Message = message, Code = code };
    }
    public static ResponseEntity OkResult(object data)
    {
        ResponseEntity response = new()
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

    public static ResponseEntity Error(string mesage, params string[] details)
    {
        return new ResponseEntity
        {
            Success = false,
            Message = mesage,
            Data = details.Length == 0 ? details.ToList() : null,
            Code = "99",
        };
    }

    public static ResponseEntity Error(string mesage, List<string> details)
    {
        return new ResponseEntity
        {
            Success = false,
            Message = mesage,
            Data = details
        };
    }

    public static ResponseEntity Error400(string message, params string[] details)
    {
        return new ResponseEntity
        {
            Code = "400",
            Success = false,
            Message = message,
            Data = details
        };
    }
    
    public static ResponseEntity Error404(string message, params string[] details)
    {
        return new ResponseEntity
        {
            Code = "404",
            Success = false,
            Message = message,
            Data = details
        };
    }

    public static ResponseEntity Error500(string message, params string[] details)
    {
        return new ResponseEntity
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