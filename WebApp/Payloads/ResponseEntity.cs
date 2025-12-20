using System.Collections;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApp.Mongo.DeserializedModel;
using X.PagedList;

namespace WebApp.Payloads;

/// <summary>
/// Basic response wrapper object
/// </summary>
public class ResponseEntity
{
    public string? Code
    {
        get; set
        {
            field = value;

            HttpCode = Code switch
            {
                null => HttpStatusCode.OK,
                "200" => HttpStatusCode.OK,
                "400" => HttpStatusCode.BadRequest,
                "401" => HttpStatusCode.Unauthorized,
                "404" => HttpStatusCode.NotFound,
                "500" => HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.InternalServerError
            };

        }
    }
    public HttpStatusCode? HttpCode { get; set; }
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

    public static ResponseEntity Error401(string message, params string[] details)
    {
        return new ResponseEntity
        {
            Code = "401",
            Success = false,
            Message = message,
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

    /// <summary>
    /// Converts the current response object to an appropriate <see cref="IActionResult"/> based on its HTTP status
    /// code.
    /// </summary>
    /// <remarks>This method is typically used in ASP.NET Core controllers to return standardized HTTP
    /// responses. The mapping ensures that the response object is wrapped in the correct <see cref="IActionResult"/>
    /// type for common status codes. For unrecognized status codes, the response defaults to a 500 Internal Server
    /// Error.</remarks>
    /// <returns>An <see cref="IActionResult"/> representing the response, mapped to the corresponding HTTP status code. The
    /// result type varies depending on the status code; for example, <see cref="OkObjectResult"/> for 200 OK, <see
    /// cref="BadRequestObjectResult"/> for 400 Bad Request, and <see cref="ObjectResult"/> with the relevant status
    /// code for other cases.</returns>
    public IActionResult ToActionResult()
    {
        return this.HttpCode switch
        {
            HttpStatusCode.OK => new OkObjectResult(this),
            HttpStatusCode.MultiStatus => new ObjectResult(this) { StatusCode = 207 },
            HttpStatusCode.BadRequest => new BadRequestObjectResult(this),
            HttpStatusCode.Unauthorized => new UnauthorizedObjectResult(this),
            HttpStatusCode.NotFound => new NotFoundObjectResult(this),
            HttpStatusCode.Forbidden => new ObjectResult(this) { StatusCode = 403 },
            HttpStatusCode.MethodNotAllowed => new ObjectResult(this) { StatusCode = 405 },
            HttpStatusCode.TooManyRequests => new ObjectResult(this) { StatusCode = 429 },
            HttpStatusCode.InternalServerError => new ObjectResult(this) { StatusCode = 500 },
            _ => new ObjectResult(this) { StatusCode = 500 }
        };
    }

}