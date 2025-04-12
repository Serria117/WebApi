using System.Globalization;
using System.Xml.Linq;
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
    /// Asynchronously retrieves files by organization and document type. The organization is determined based on the user's working organization.
    /// </summary>
    /// <param name="documentType">The type of the document to filter by.</param>
    /// <param name="requestParam">The request parameters for pagination and filtering.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> containing a paginated list of documents that match the specified criteria.
    /// </returns>
    Task<AppResponse> GetFilesByTypeAsync(DocumentType documentType, RequestParam requestParam);

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
    Task<AppResponse> GetDocumentByIdAsync(int documentId);

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
    Task<AppResponse> ReadDocumentFromFileAsync(int docId);
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
        var relativeUploadDir = Path.Combine("Uploads", org.TaxId);
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
            //var filePath = Path.Combine(uploadDir, uniqueFileName);
            var relativeFilePath = Path.Combine(relativeUploadDir, uniqueFileName);
            var absoluteFilePath = Path.Combine(env.ContentRootPath, relativeFilePath);
            var doc = await XDocument.LoadAsync(memoryStream, LoadOptions.None, CancellationToken.None);
            var hash = CalculateMd5Hash(doc);
            var type = GetDocumentTypeFromXml(doc);

            var soLan = doc.GetXmlNodeValue("soLan");
            var docDate = doc.GetXmlNodeValue("ngayLapTKhai");

            // save the file to the stream
            await using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
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
                FilePath = relativeFilePath,
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

    public async Task<AppResponse> GetFilesByTypeAsync(DocumentType documentType,
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

        var data = files.MapPagedList(f => new
        {
            f.Id,
            f.FileName,
            f.FilePath,
            f.UploadTime,
            f.DocumentType,
            Name = f.DocumentName,
            f.Period,
            f.NumberOfAdjustment,
            f.PeriodType,
            f.DocumentDate,
            f.AdjustmentType
        });

        return AppResponse.SuccessResponse(data);
    }


    public async Task<AppResponse> GetDocumentByIdAsync(int documentId)
    {
        var file = await docRepository
                         .Find(filter: x => x.Organization.Id.ToString() == WorkingOrg && x.Id == documentId,
                               include: nameof(OrgDocument.Organization))
                         .FirstOrDefaultAsync();
        return file is not null
            ? AppResponse.SuccessResponse(file.FilePath)
            : AppResponse.Error404("Document not found");
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

            var filePath = Path.Combine(env.ContentRootPath, doc.FilePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
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

    public async Task<AppResponse> ReadDocumentFromFileAsync(int docId)
    {
        try
        {
            var doc = await docRepository.Find(f => f.Organization.Id.ToString() == WorkingOrg && f.Id == docId,
                                               nameof(OrgDocument.Organization))
                                         .FirstOrDefaultAsync();
            if (doc is null) return AppResponse.Error404("Document Id doesn't exist");
            var filePath = GetFilePath(doc);
            if (!File.Exists(filePath)) return AppResponse.Error404("The Document may have been deleted or moved");

            var docType = doc.DocumentType;

            object? data = docType switch
            {
                DocumentType.TK_01GTGT => await Read_01GTGT_Document(doc),
                DocumentType.TK_05KK_TNCN => await Read_05KK_TNCN_Document(doc),
                DocumentType.TK_05QTN_TNCN => await Read_05QTN_TNCN_Document(doc),
                _ => null
            };

            return data is not null
                ? AppResponse.SuccessResponse(data)
                : AppResponse.Error400("Document type is not supported");
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return AppResponse.Error400("Can not find the document");
        }
    }

    #region Read Document Details

    private async Task<Document01GtgtPayload> Read_01GTGT_Document(OrgDocument doc)
    {
        string filePath = GetFilePath(doc);
        await using var stream = new FileStream(filePath, FileMode.Open);
        var xDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        var data = new Document01GtgtPayload
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
        return data;
    }

    private async Task<Document05KkPayload> Read_05KK_TNCN_Document(OrgDocument doc)
    {
        string filePath = GetFilePath(doc);
        await using var stream = new FileStream(filePath, FileMode.Open);
        var xDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        return new Document05KkPayload()
        {
            Ct15 = xDocument.GetXmlNodeValueAsLong("c15"),
            Ct16 = xDocument.GetXmlNodeValueAsLong("c16"),
            Ct17 = xDocument.GetXmlNodeValueAsLong("c17"),
            Ct18 = xDocument.GetXmlNodeValueAsLong("c18"),
            Ct19 = xDocument.GetXmlNodeValueAsLong("c19"),
            Ct20 = xDocument.GetXmlNodeValueAsLong("c20"),
            Ct21 = xDocument.GetXmlNodeValueAsLong("c21"),
            Ct22 = xDocument.GetXmlNodeValueAsLong("c22"),
            Ct23 = xDocument.GetXmlNodeValueAsLong("c23"),
            Ct24 = xDocument.GetXmlNodeValueAsLong("c24"),
            Ct25 = xDocument.GetXmlNodeValueAsLong("c25"),
            Ct26 = xDocument.GetXmlNodeValueAsLong("c26"),
            Ct27 = xDocument.GetXmlNodeValueAsLong("c27"),
            Ct28 = xDocument.GetXmlNodeValueAsLong("c28"),
            Ct29 = xDocument.GetXmlNodeValueAsLong("c29"),
            Ct30 = xDocument.GetXmlNodeValueAsLong("c30"),
            Ct31 = xDocument.GetXmlNodeValueAsLong("c31"),
            Ct32 = xDocument.GetXmlNodeValueAsLong("c32"),
        };
    }

    //TODO: Read 05QTN_TNCN document
    private async Task<Document05QTNPayload> Read_05QTN_TNCN_Document(OrgDocument doc)
    {
        string filePath = GetFilePath(doc);
        await using var stream = new FileStream(filePath, FileMode.Open);
        var xDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        XNamespace ns = "http://kekhaithue.gdt.gov.vn/TKhaiThue";

        var list = xDocument.Descendants(ns + "PLuc_05_1_BK_QTT")
                            .Elements(ns + "BKeCTietCNhan")
                            .Select(x => new Pluc051Ct
                            {
                                Id = x.Attribute("id")?.ToString(),
                                Ct07 = x.Element(ns + "ct07")?.Value,
                                Ct08 = x.Element(ns + "ct08")?.Value,
                                Ct10 = x.GetValueFromElementAsLong("ct10", ns),
                                Ct11 = x.GetValueFromElementAsLong("ct11", ns),
                                Ct12 = x.GetValueFromElementAsLong("ct12", ns),
                                Ct13 = x.GetValueFromElementAsLong("ct13", ns),
                                Ct14 = x.GetValueFromElementAsLong("ct14", ns),
                                Ct15 = x.GetValueFromElementAsLong("ct15", ns),
                                Ct16 = x.GetValueFromElementAsLong("ct16", ns),
                                Ct17 = x.GetValueFromElementAsLong("ct17", ns),
                                Ct18 = x.GetValueFromElementAsLong("ct18", ns),
                                Ct19 = x.GetValueFromElementAsLong("ct19", ns),
                                Ct20 = x.GetValueFromElementAsLong("ct20", ns),
                                Ct21 = x.GetValueFromElementAsLong("ct21", ns),
                                Ct22 = x.GetValueFromElementAsLong("ct22", ns),
                                Ct23 = x.GetValueFromElementAsLong("ct23", ns),
                                Ct24 = x.GetValueFromElementAsLong("ct24", ns),
                                Ct25 = x.GetValueFromElementAsLong("ct25", ns),
                                Ct26 = x.GetValueFromElementAsLong("ct26", ns),
                                Ct27 = x.GetValueFromElementAsLong("ct27", ns),
                            }).ToList();
        var pl051 = xDocument.Descendants(ns + "PLuc_05_1_BK_QTT")
                             .Select(x => new Pl051Bk()
                             {
                                 Pluc051Cts = list,
                                 Ct28 = x.GetValueFromElementAsLong("ct28", ns),
                                 Ct29 = x.GetValueFromElementAsLong("ct29", ns),
                                 Ct30 = x.GetValueFromElementAsLong("ct30", ns),
                                 Ct31 = x.GetValueFromElementAsLong("ct31", ns),
                                 Ct32 = x.GetValueFromElementAsLong("ct32", ns),
                                 Ct33 = x.GetValueFromElementAsLong("ct33", ns),
                                 Ct34 = x.GetValueFromElementAsLong("ct34", ns),
                                 Ct35 = x.GetValueFromElementAsLong("ct35", ns),
                                 Ct36 = x.GetValueFromElementAsLong("ct36", ns),
                                 Ct37 = x.GetValueFromElementAsLong("ct37", ns),
                                 Ct38 = x.GetValueFromElementAsLong("ct38", ns),
                                 Ct39 = x.GetValueFromElementAsLong("ct39", ns),
                                 Ct40 = x.GetValueFromElementAsLong("ct40", ns),
                                 Ct41 = x.GetValueFromElementAsLong("ct41", ns),
                                 Ct42 = x.GetValueFromElementAsLong("ct42", ns),
                                 Ct43 = x.GetValueFromElementAsLong("ct43", ns),
                             })
                             .FirstOrDefault();
        return new Document05QTNPayload()
        {
            Pluc051Bk = pl051,
            Ct16 = xDocument.GetXmlNodeValueAsLong("ct16"),
            Ct17 = xDocument.GetXmlNodeValueAsLong("ct17"),
            Ct18 = xDocument.GetXmlNodeValueAsLong("ct18"),
            Ct19 = xDocument.GetXmlNodeValueAsLong("ct19"),
            Ct20 = xDocument.GetXmlNodeValueAsLong("ct20"),
            Ct21 = xDocument.GetXmlNodeValueAsLong("ct21"),
            Ct22 = xDocument.GetXmlNodeValueAsLong("ct22"),
            Ct23 = xDocument.GetXmlNodeValueAsLong("ct23"),
            Ct24 = xDocument.GetXmlNodeValueAsLong("ct24"),
            Ct25 = xDocument.GetXmlNodeValueAsLong("ct25"),
            Ct26 = xDocument.GetXmlNodeValueAsLong("ct26"),
            Ct27 = xDocument.GetXmlNodeValueAsLong("ct27"),
            Ct28 = xDocument.GetXmlNodeValueAsLong("ct28"),
            Ct29 = xDocument.GetXmlNodeValueAsLong("ct29"),
            Ct30 = xDocument.GetXmlNodeValueAsLong("ct30"),
            Ct31 = xDocument.GetXmlNodeValueAsLong("ct31"),
            Ct32 = xDocument.GetXmlNodeValueAsLong("ct32"),
            Ct33 = xDocument.GetXmlNodeValueAsLong("ct33"),
            Ct34 = xDocument.GetXmlNodeValueAsLong("ct34"),
            Ct35 = xDocument.GetXmlNodeValueAsLong("ct35"),
            Ct36 = xDocument.GetXmlNodeValueAsLong("ct36"),
            Ct37 = xDocument.GetXmlNodeValueAsLong("ct37"),
            Ct38 = xDocument.GetXmlNodeValueAsLong("ct38"),
            Ct39 = xDocument.GetXmlNodeValueAsLong("ct39"),
            Ct40 = xDocument.GetXmlNodeValueAsLong("ct40"),
            Ct41 = xDocument.GetXmlNodeValueAsLong("ct41"),
        };
    }
    //TODO: Read BCTC_133 document

    //TODO: Read TK_03TNDN document

    #endregion

    private string GetFilePath(OrgDocument doc)
    {
        var filePath = Path.Combine(env.ContentRootPath, doc.FilePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The File at <{filePath}> has been move or deleted");
        return filePath;
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