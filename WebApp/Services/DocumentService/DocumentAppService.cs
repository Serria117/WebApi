using System.Globalization;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Payloads.DocumentPayload;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.UserService;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.DocumentService;

public interface IDocumentAppService
{
    
    /// <summary>
    /// Asynchronously retrieves files by organization and document type.
    /// </summary>
    /// <param name="documentType">The type of the document to filter by.</param>
    /// <param name="requestParam">The request parameters for pagination and filtering.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> containing a paginated list of documents that match the specified criteria.
    /// </returns>
    Task<AppResponse> GetFilesByOrgAndTypeAsync(DocumentType documentType, RequestParam requestParam);

    /// <summary>
    /// Asynchronously uploads document files for a specified organization.
    /// </summary>
    /// <param name="files">A list of files to be uploaded.</param>
    /// <returns>An <see cref="AppResponse"/> indicating the result of the operation, including the number of files uploaded successfully and any validation errors.</returns>
    /// <remarks>
    /// Validates each file against the organization's criteria, saves them to a designated directory,
    /// and extracts metadata from the files for further processing. If no files are provided or the organization is not found,
    /// an error response is returned.
    /// </remarks>
    Task<AppResponse> UploadDocFileAsync(List<IFormFile> files);
    
    /// <summary>
    /// Asynchronously retrieves a file path by its document ID for the current working organization.
    /// </summary>
    /// <param name="documentId">The ID of the document to retrieve.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> containing the file path if found, 
    /// or a 404 error response if the document is not found.
    /// </returns>
    Task<AppResponse> GetFileByIdAsync(int documentId);
    
    /// <summary>
    /// Deletes a document file by its ID.
    /// </summary>
    /// <param name="documentId">The ID of the document to delete.</param>
    /// <returns>An <see cref="AppResponse"/> indicating the result of the operation.
    /// Returns a 404 error if the document is not found, a 500 error if an exception occurs,
    /// or a success response if the file is deleted successfully.</returns>
    Task<AppResponse> DeleteFileByIdAsync(int documentId);

    /// <summary>
    /// Reads and processes the 01GTGT XML document by its document ID.
    /// </summary>
    /// <param name="docId">The ID of the document to read.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> containing the processed data if the document is found and valid,
    /// or an error response if the document is not found or invalid.
    /// </returns>
    Task<AppResponse> Read_01GTGT_Xml(int docId);
}

public class DocumentAppService(IAppRepository<OrgDocument, int> docRepository,
                                IAppRepository<Organization, Guid> orgRepository,
                                ILogger<DocumentAppService> logger,
                                IUserManager userManager,
                                IHostEnvironment env) : AppServiceBase(userManager), IDocumentAppService
{
    public async Task<AppResponse> UploadDocFileAsync(List<IFormFile> files)
    {
        if (files.Count == 0) return AppResponse.Error("No file uploaded");
        (bool result, Guid orgId) = GetId(WorkingOrg);
        if (!result)
        {
            return AppResponse.Error400("You must select working organization first");
        }

        var org = await orgRepository.FindByIdAsync(orgId);
        if (org is null) return AppResponse.Error404("Organization not found");
        var uploadFiles = new List<OrgDocument>();
        var uploadDir = Path.Combine(env.ContentRootPath, "Uploads", org.TaxId);

        if (!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }

        var validationError = new List<string>();

        foreach (var file in files.Where(file => file.Length != 0))
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var validated = await ValidateDocument(org, memoryStream, file.FileName);
            if (!(validated.Result))
            {
                validationError.Add(validated.Error);
                continue;
            }
            memoryStream.Position = 0;
            var uniqueFileName = $"{file.FileName}_{Ulid.NewUlid()}.xml";
            var filePath = Path.Combine(uploadDir, uniqueFileName);
            // read document and extract some extra metadata:
            // await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var doc = await XDocument.LoadAsync(memoryStream, LoadOptions.None, CancellationToken.None);
            var hash = CalculateMd5Hash(doc);
            var type = GetDocumentTypeFromXml(doc);
            
            var soLan = doc.GetXmlNodeValue("soLan");
            var docDate = doc.GetXmlNodeValue("ngayLapTKhai");

            // save the file to the stream
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // create the document object
            var uploadFile = new OrgDocument
            {
                Organization = org,
                DocumentType = type,
                DocumentName = doc.GetXmlNodeValue("tenTKhai"),
                DocumentDate = docDate == null
                    ? null
                    : DateTime.ParseExact(docDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                AdjustmentType = doc.GetXmlNodeValue("loaiTKhai"),
                NumberOfAdjustment = soLan is null ? 0 : int.Parse(soLan),
                Period = doc.GetXmlNodeValue("kyKKhai"),
                PeriodType = doc.GetXmlNodeValue("kieuKy"),
                FileName = file.FileName,
                FilePath = filePath,
                UploadTime = DateTime.Now,
                FileSize = file.Length,
                Hash = hash
            };

            uploadFiles.Add(uploadFile);
        }

        var total = uploadFiles.Count;
        await docRepository.CreateManyAsync(uploadFiles);
        return AppResponse.SuccessResponse(new
        {
            message = $"{total} file(s) uploaded successfully.",
            total,
            error = validationError.Count,
            validationError
        });


    }

    public async Task<AppResponse> GetFilesByOrgAndTypeAsync(DocumentType documentType,
                                                             RequestParam requestParam)
    {
        (bool result, Guid orgId) = GetId(WorkingOrg);
        if (!result)
        {
            return AppResponse.Error400("You must select working organization first");
        }

        var org = await orgRepository.FindByIdAsync(orgId);
        if (org is null) return AppResponse.Error404("Organization not found");
        var pageRequest = PageRequest.GetPage(requestParam);
        var files = await docRepository.FindAndSort(x => x.Organization.Id == orgId && x.DocumentType == documentType,
                                                    [],
                                                    ["CreateAt DESC"])
                                       .ToPagedListAsync(pageRequest.Number, pageRequest.Size);

        return AppResponse.SuccessResponse(files.MapPagedList(f => new
        {
            f.Id,
            f.FileName,
            f.FilePath,
            f.UploadTime,
            DocumentType = f.DocumentType.ToString(),
            Name = f.DocumentName,
            f.Period,
            f.NumberOfAdjustment,
            f.PeriodType,
            f.DocumentDate,
        }));
    }

    
    public async Task<AppResponse> GetFileByIdAsync(int documentId)
    {
        var file = await docRepository.Find(filter: x => x.Organization.Id.ToString() == WorkingOrg && x.Id == documentId,
                                         include: nameof(OrgDocument.Organization))
                                   .FirstOrDefaultAsync();
        return file is not null ? AppResponse.SuccessResponse(file.FilePath) : AppResponse.Error404("Document not found");
    }

    public async Task<AppResponse> DeleteFileByIdAsync(int documentId)
    {
        try
        {
            var doc = await docRepository.Find(f => f.Organization.Id.ToString() == WorkingOrg 
                                                    && f.Id == documentId,
                                               nameof(OrgDocument.Organization))
                                         .FirstOrDefaultAsync();
            if (doc is null)
            {
                return AppResponse.Error404("Document not found");
            }

            if (File.Exists(doc.FilePath))
            {
                File.Delete(doc.FilePath);
            }

            return (await docRepository.HardDeleteAsync(documentId))
                ? AppResponse.Ok()
                : AppResponse.Error500("Something went wrong");
        }
        catch (Exception e)
        {
            return AppResponse.Error500("Error while deleting file", e.Message);
        }
    }

    public async Task<AppResponse> Read_01GTGT_Xml(int docId)
    {
        var doc = await docRepository.Find(f => f.Organization.Id.ToString() == WorkingOrg && f.Id == docId,
                                           nameof(OrgDocument.Organization))
                                     .FirstOrDefaultAsync();
        if (doc is null) return AppResponse.Error404("Document Id doesn't exist");
        if (!File.Exists(doc.FilePath)) return AppResponse.Error404("The Document may have been deleted or moved");
        if (doc.DocumentType != DocumentType.TK_01GTGT) return AppResponse.Error400("Please choose 01/GTGT document");
        await using var stream = new FileStream(doc.FilePath, FileMode.Open);
        var xDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        var data = new VatDocumentPayload
        {
            OrganizationName = xDocument.GetXmlNodeValue("tenNNT") ?? string.Empty,
            TaxId = xDocument.GetXmlNodeValue("mst") ?? string.Empty,
            DocumentName = xDocument.GetXmlNodeValue("tenTKhai"),
            Address = xDocument.GetXmlNodeValue("dchiNNT"),
            Ct21 = xDocument.GetXmlNodeValueAsLong("ct21"),
            Ct22 = xDocument.GetXmlNodeValueAsLong("ct22"),
            Ct23 = xDocument.GetXmlNodeValueAsLong("ct23"),
            Ct23a = xDocument.GetXmlNodeValueAsLong("ct23a"),
            Ct24 = xDocument.GetXmlNodeValueAsLong("ct24"),
            Ct24a = xDocument.GetXmlNodeValueAsLong("ct24a"),
            Ct25 = xDocument.GetXmlNodeValueAsLong("ct25"),
            Ct26 = xDocument.GetXmlNodeValueAsLong("ct26"),
            Ct27 = xDocument.GetXmlNodeValueAsLong("ct27"),
            Ct28 = xDocument.GetXmlNodeValueAsLong("ct28"),
            Ct29 = xDocument.GetXmlNodeValueAsLong("ct29"),
            Ct30 = xDocument.GetXmlNodeValueAsLong("ct30"),
            Ct31 = xDocument.GetXmlNodeValueAsLong("ct31"),
            Ct32 = xDocument.GetXmlNodeValueAsLong("ct32"),
            Ct32a = xDocument.GetXmlNodeValueAsLong("ct32a"),
            Ct33 = xDocument.GetXmlNodeValueAsLong("ct33"),
            Ct34 = xDocument.GetXmlNodeValueAsLong("ct34"),
            Ct35 = xDocument.GetXmlNodeValueAsLong("ct35"),
            Ct36 = xDocument.GetXmlNodeValueAsLong("ct36"),
            Ct37 = xDocument.GetXmlNodeValueAsLong("ct37"),
            Ct38 = xDocument.GetXmlNodeValueAsLong("ct38"),
            Ct39a = xDocument.GetXmlNodeValueAsLong("ct39a"),
            Ct40 = xDocument.GetXmlNodeValueAsLong("ct40"),
            Ct40a = xDocument.GetXmlNodeValueAsLong("ct40a"),
            Ct40b = xDocument.GetXmlNodeValueAsLong("ct40b"),
            Ct41 = xDocument.GetXmlNodeValueAsLong("ct41"),
            Ct42 = xDocument.GetXmlNodeValueAsLong("ct42"),
            Ct43 = xDocument.GetXmlNodeValueAsLong("ct43"),
            Ct44 = xDocument.GetXmlNodeValueAsLong("ct44"),
        };

        return AppResponse.SuccessResponse(data);
    }

    private DocumentType GetDocumentTypeFromXml(XDocument doc)
    {
        try
        {
            // find the node 'maTKhai' to extract document type:
            var maTKhai = doc.GetXmlNodeValue("maTKhai");
            if (string.IsNullOrEmpty(maTKhai))
            {
                throw new Exception("maTKhai not found");
            }

            return maTKhai switch
            {
                "892" => DocumentType.TK_03TNDN,
                "842" => DocumentType.TK_01GTGT,
                "864" => DocumentType.TK_05KK_TNCN,
                "953" => DocumentType.TK_05QTN_TNCN,
                "683" => DocumentType.TK_BCTC_133,
                _ => DocumentType.General_doc
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while getting document type from xml");
            return DocumentType.Undefined;
        }
    }

    private async Task<(bool Result, string Error)> ValidateDocument(Organization org, Stream stream, string fileName)
    {
        try
        {
            //await using var stream = file.OpenReadStream();
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
            var taxId = document.GetXmlNodeValue("mst");
            var result = taxId is null || taxId == org.TaxId;
            logger.LogInformation("Validated document with tax ID: {taxId}", taxId);
            return result
                ? (result, string.Empty)
                : (result, $"Tax ID does not match the organization's tax ID in file: {fileName}");
        }
        catch (Exception e)
        {
            logger.LogError("Error while validating document. Caused by: {}", e.Message);
            return (false, e.Message);
        }
    }
    
    private static string CalculateMd5Hash(XDocument doc)
    {
        // Chuẩn hóa XML (loại bỏ khoảng trắng không cần thiết)
        var normalizedXml = doc.ToString(SaveOptions.DisableFormatting);
        
        var bytes = System.Text.Encoding.UTF8.GetBytes(normalizedXml);
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hashBytes = md5.ComputeHash(bytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
    
}