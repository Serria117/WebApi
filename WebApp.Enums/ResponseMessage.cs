namespace WebApp.Enums;

public struct ResponseMessage
{
    public const string Ok = "OK";
    public const string Error = "ERROR";
    public const string BadRequest = "BAD_REQUEST";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string InternalServerError = "INTERNAL_ERROR";
    
    public const string IdNotFound = "Id không hợp lệ hoặc không tồn tại";
    public const string Duplicate = "DUPLICATE";
}