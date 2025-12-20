using System.Runtime.CompilerServices;

namespace WebApp.Enums;

/// <summary>
/// Represents a set of predefined system constraints and limits.<br/>
/// This struct contains constant values that define maximum allowed sizes
/// or limits for various operations, ensuring the application adheres to
/// the specified boundaries and avoids exceeding system constraints.
/// </summary>
public struct SystemBoundary
{
    public const int MaxBatchSize = 100;
    
    public const int MinPageSize = 10;
    public const int MaxPageSize = 1000;

    public const int MinPageIndex = 1;
    public const int MaxPageIndex = 100;
    
    public const int MaxFileSize = 1024 * 1024 * 50;
    
    public const int TimeOut = 30000;

    public static readonly string[] AllowFileExt = ["xlsx", "xml", "docx", "pdf", "jpg", "jpeg", "png", "gif"];

    public const string RandomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
}