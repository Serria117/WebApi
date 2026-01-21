using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Enums;

/// <summary>
/// Represents a struct that contains constants for folder names used in the application.
/// </summary>
/// <remarks>
/// This struct provides predefined folder name constants for use in file operations
/// such as uploads, downloads, and template management across the application.
/// </remarks>
public struct FolderName
{
    public const string ExportTemplate = "ExportTemplates";
    public const string ImportTemplate = "ImportTemplates";
    public const string DocumentTemplate = "DocumentTemplates";
    public const string Uploads = "Uploads";
    public const string ExcelPayroll = "ExcelPayroll";
}
