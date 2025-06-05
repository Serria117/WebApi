using System.Globalization;
using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Spire.Xls;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Payloads.DocumentPayload;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.DocumentService.Dto;
using WebApp.Services.Mappers;
using WebApp.Services.UserService;
using X.Extensions.PagedList.EF;

// ReSharper disable NotAccessedVariable

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
    Task<AppResponse> FindDocumentsAsync(DocumentType documentType, RequestParam requestParam);

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

    Task<(string FileName, byte[] File)> ConsolidateVatDocumentsAsync(List<int> ids);

    /// <summary>
    /// Check a file for duplicated and returns true if it exists
    /// </summary>
    /// <param name="hash">The MD5 hash value of the file to check for duplication</param>
    /// <returns>True if the file already exists, false otherwise</returns>
    Task<bool> CheckHashForDuplicated(string hash);

    Task<string> ComputeHashFromFile(IFormFile file);

    /// <summary>
    /// Check multiple files for duplicated and returns a list of duplicates
    /// This is best suited when uploading multiple files at once
    /// </summary>
    /// <param name="hashes">Collection of hashes to check for duplicates</param>
    /// <returns>Collection of duplicate hashes</returns>
    Task<List<string>> CheckMultiFilesForDuplicated(List<string> hashes);
    Task<AppResponse> ReadXmlToStringAsync(int id);
}

public class DocumentBaseAppService(IAppRepository<OrgDocument, int> docRepository,
                                IAppRepository<Organization, Guid> orgRepository,
                                ILogger<DocumentBaseAppService> logger,
                                IUserManager userManager,
                                IHostEnvironment env) : BaseAppService(userManager), IDocumentAppService
{
    public async Task<AppResponse> UploadDocFileAsync(List<IFormFile> files)
    {
        if (files.Count == 0) return AppResponse.Error("No file uploaded");
        var oId = WorkingOrg.ToGuid();
        if (oId == Guid.Empty)
        {
            return AppResponse.Error400("You must select working organization first");
        }

        var org = await orgRepository.FindByIdAsync(oId);
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
            var hash = await Md5HashAsync(memoryStream);
            var validated = await ValidateDocument(org, memoryStream, file.FileName);
            if (!(validated.Result))
            {
                validationError.Add(validated.Error);
                continue;
            }

            memoryStream.Position = 0;
            var uniqueFileName = $"{file.FileName}_{hash.ToUpper()}.xml";
            //var filePath = Path.Combine(uploadDir, uniqueFileName);
            var relativeFilePath = Path.Combine(relativeUploadDir, uniqueFileName);
            var absoluteFilePath = Path.Combine(env.ContentRootPath, relativeFilePath);
            var doc = await XDocument.LoadAsync(memoryStream, LoadOptions.None, CancellationToken.None);
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
                DocumentDate = docDate?.ToDateTime(),
                AdjustmentType = doc.GetXmlNodeValue("loaiTKhai"),
                NumberOfAdjustment = soLan is null ? 0 : int.Parse(soLan),
                Period = doc.GetXmlNodeValue("kyKKhai").GetPeriod().Period,
                Year = doc.GetXmlNodeValue("kyKKhai").GetPeriod().Year,
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
        return AppResponse.OkResult(new
        {
            message = $"{total} file(s) uploaded successfully.",
            total,
            error = validationError.Count,
            validationError
        });
    }

    public async Task<AppResponse> FindDocumentsAsync(DocumentType documentType,
                                                      RequestParam requestParam)
    {
        var oId = WorkingOrg.ToGuid();
        if (oId == Guid.Empty)
        {
            return AppResponse.Error400("You must select working organization first");
        }

        var param = PageRequest.BuildRequest(requestParam);

        int fromYear;
        int toYear;

        var org = await orgRepository.FindByIdAsync(oId);
        if (org is null) return AppResponse.Error404("Organization not found");

        var basedQuery = docRepository.FindAndSort(filter: x => x.Organization.Id == oId
                                                                && x.DocumentType == documentType,
                                                   include: [],
                                                   sortBy: [$"{nameof(OrgDocument.DocumentDate)} {SortOrder.ASC}"]);
        var filteredQuery = basedQuery;

        if (param is { To: not null, From: not null })
        {
            fromYear = int.Parse(param.From); //TODO: handle parsing failed
            toYear = int.Parse(param.To);
            filteredQuery = basedQuery.Where(x => x.Year >= fromYear && x.Year <= toYear);
        }

        var files = await filteredQuery.ToPagedListAsync(param.Page, param.Size);

        var dtoList = files.MapPagedList(f => new DocumentDisplayDto
        {
            Id = f.Id,
            FileName = f.FileName,
            FilePath = f.FilePath,
            UploadTime = f.UploadTime,
            DocumentType = f.DocumentType,
            Name = f.DocumentName,
            Period = f.Period,
            Year = f.Year,
            NumberOfAdjustment = f.NumberOfAdjustment ?? 0,
            PeriodType = f.PeriodType,
            DocumentDate = f.DocumentDate ?? DateTime.Now,
            AdjustmentType = f.AdjustmentType
        });
        return AppResponse.OkResult(dtoList);
    }

    public async Task<AppResponse> GetDocumentByIdAsync(int documentId)
    {
        var file = await docRepository
                         .Find(filter: x => x.Organization.Id.ToString() == WorkingOrg && x.Id == documentId,
                               include: nameof(OrgDocument.Organization))
                         .FirstOrDefaultAsync();
        return file is not null
            ? AppResponse.OkResult(file.FilePath)
            : AppResponse.Error404("Document not found");
    }

    public async Task<AppResponse> ReadXmlToStringAsync(int id)
    {
        var file = await docRepository
                         .Find(filter: x => x.Organization.Id.ToString() == WorkingOrg && x.Id == id,
                               include: nameof(OrgDocument.Organization))
                         .FirstOrDefaultAsync() ?? throw new NotFoundException($"Document [{id}] not found on the server.");
        var filePath = GetFilePath(file);
        using var stream = new FileStream(filePath, FileMode.Open);
        var xmlDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        return AppResponse.OkResult(xmlDocument.ToString());
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
                ? AppResponse.OkResult(data)
                : AppResponse.Error400("Document type is not supported");
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return AppResponse.Error400("Can not find the document");
        }
    }

    public async Task<bool> CheckHashForDuplicated(string hash)
    {
        var doc = await docRepository.Find(f => f.Hash == hash).FirstOrDefaultAsync();
        return doc is not null;
    }

    public async Task<List<string>> CheckMultiFilesForDuplicated(List<string> hashes)
    {
        var docs = await docRepository.Find(f => hashes.Contains(f.Hash))
                                      .ToListAsync(); //find all documents with same hashes
        return docs.Select(x => x.Hash).ToList(); //return duplicate hashes
    }

    public async Task<string> ComputeHashFromFile(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0; // reset position back to start
        return await Md5HashAsync(memoryStream);
    }

    #region Read Document Details

    private async Task<Document01GtgtPayload?> Read_01GTGT_Document(OrgDocument doc)
    {
        try
        {
            string filePath = GetFilePath(doc);
            await using var stream = new FileStream(filePath, FileMode.Open);
            var xDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
            var data = new Document01GtgtPayload
            {
                OrganizationName = xDocument.GetXmlNodeValue("tenNNT") ?? string.Empty,
                TaxId = xDocument.GetXmlNodeValue("mst") ?? string.Empty,
                DocumentName = xDocument.GetXmlNodeValue("tenTKhai"),
                Period = doc.Period,
                Year = doc.Year,
                PeriodType = doc.PeriodType,
                NumberOfAdjustment = doc.NumberOfAdjustment ?? 0,
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
        catch (Exception e)
        {
            logger.LogInformation("Exception was caused: {exception}", e.GetType().Name);
            logger.LogError("Error while reading document Id: [{id}]. {message}", doc.Id, e.Message);
            return null;
        }
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

    #region 01GTGT Documents

    public async Task<(string FileName, byte[] File)> ConsolidateVatDocumentsAsync(List<int> ids)
    {
        var docList = await docRepository.FindAndSort(filter: x => ids.Contains(x.Id)
                                                                   && x.Organization.Id.ToString() == WorkingOrg,
                                                      include: [nameof(OrgDocument.Organization)],
                                                      sortBy: ["DocumentDate ASC"]) //sort by date ascending
                                         .ToListAsync();
        if (docList.Count == 0)
        {
            throw new EmptyResultException("Document not found");
        }
        //Asynchrously read all documents into memory to save time
        var payloads = await this.Get01GtgtPayloads(docList);
        if (payloads.Count == 0)
        {
            throw new EmptyResultException("No document on disk to read. Check the file path.");
        }
        
        var wb = new Workbook { Version = ExcelVersion.Version2016 };
        var sh = wb.Worksheets[0];
        sh.Name = "Sheet1";
        sh.Range["A1"].Value = "Tổng hợp tờ khai GTGT";
        sh.Range["A1"].Style.Font.IsBold = true;
        sh.Range["A1"].Style.Font.Size = 16;
        sh.Range["A2"].Value = $"{docList[0].Organization.FullName} - {docList[0].Organization.TaxId}".ToUpper();
        sh.Range["A2"].Style.Font.IsBold = true;
        const int headerRow = 4;

        //Insert header rows:
        string[] headers =
        [
            "Kỳ thuế", //1
            "Loại tờ khai", //2
            "Lần nộp", //3
            "Ngày tờ khai", //4
            "Thuế GTGT khấu trừ kỳ trước - CT22", //5
            "Giá trị HHDV mua vào trong kỳ - CT23", //6
            "Thuế GTGT được khấu trừ kỳ này - CT25", //7
            "Doanh thu HHDV bán ra chịu thuế - CT27", //8
            "Doanh thu HHDV 5% - CT30", //9
            "Thuế GTGT HHDV 5% - CT31", //10
            "Doanh thu HHDV 10% - CT32", //11
            "Thuế GTGT HHDV 10% - CT33", //12
            "Thuế GTGT phát sinh - CT36", //13
            "Điều chỉnh giảm - CT37", //14
            "Điều chỉnh tăng - CT38", //15
            "Thuế GTGT phải nộp - CT40", //16
            "Thuế GTGT chưa khấu trừ hết - CT41", //17
            "Thuế GTGT còn được khấu trừ - CT43", //18
            "Giải trình bổ sung" //19
        ];
        //TODO: add more columns here
        for (int i = 1; i <= headers.Length; i++)
        {
            sh.Range[headerRow, i].Value = headers[i - 1];
            sh.Range[headerRow, i].BorderAround(LineStyleType.Thin);
            sh.Range[headerRow, i].HorizontalAlignment = HorizontalAlignType.Center;
            sh.Range[headerRow, i].VerticalAlignment = VerticalAlignType.Center;
            sh.Range[headerRow, i].Style.Font.IsBold = true;
            sh.Range[headerRow, i].IsWrapText = true;
            sh.Range[headerRow, i].ColumnWidth = 15;
        }

        // loop through all documents and consolidate them into one excel file, require reading each document's file
        var index = headerRow + 1;
        var processedDocs = 0;
        
        //Read all documents and create a dictionary of payloads
        
        foreach (var payload in payloads)
        {
            var doc = payload.Key;
            var data = payloads[doc];
            
            sh.Range[index, 1].Value2 = $"{doc.PeriodType}{doc.Period}/{doc.Year}";
            sh.Range[index, 2].Value2 = doc.AdjustmentType switch
            {
                "C" => "Chính thức",
                "B" => "Bổ sung",
                _ => "Không xác định"
            };
            sh.Range[index, 3].Value2 = doc.NumberOfAdjustment;
            sh.Range[index, 4].Value2 = doc.DocumentDate?.ToLocalTime();
            sh.Range[index, 4].Style.NumberFormat = "dd/mm/yyyy";
            sh.Range[index, 5].Value2 = data.Ct22;
            sh.Range[index, 6].Value2 = data.Ct23;
            sh.Range[index, 7].Value2 = data.Ct25;
            sh.Range[index, 8].Value2 = data.Ct27;
            sh.Range[index, 9].Value2 = data.Ct30;
            sh.Range[index, 10].Value2 = data.Ct31;
            sh.Range[index, 11].Value2 = data.Ct32;
            sh.Range[index, 12].Value2 = data.Ct33;
            sh.Range[index, 13].Value2 = data.Ct36;
            sh.Range[index, 14].Value2 = data.Ct37;
            sh.Range[index, 16].Value2 = data.Ct38;
            sh.Range[index, 16].Value2 = data.Ct40;
            sh.Range[index, 17].Value2 = data.Ct41;
            sh.Range[index, 18].Value2 = data.Ct43;

            for (var i = 1; i <= headers.Length; i++)
            {
                sh.Range[index, i].BorderAround(LineStyleType.Thin);
            }

            for (var i = 5; i <= headers.Length; i++)
            {
                sh.Range[index, i].NumberFormat = "#,##0";
            }

            index++; //increment row index
            processedDocs++;
            
        }
        
        /*foreach (OrgDocument doc in docList)
        {
            var data = await Get01GtgtPayload(doc.Id);
            if (data is null)
            {
                continue;
            }

            sh.Range[index, 1].Value2 = $"{doc.PeriodType}{doc.Period}/{doc.Year}";
            sh.Range[index, 2].Value2 = doc.AdjustmentType switch
            {
                "C" => "Chính thức",
                "B" => "Bổ sung",
                _ => "Không xác định"
            };
            sh.Range[index, 3].Value2 = doc.NumberOfAdjustment;
            sh.Range[index, 4].Value2 = doc.DocumentDate?.ToLocalTime();
            sh.Range[index, 4].Style.NumberFormat = "dd/mm/yyyy";
            sh.Range[index, 5].Value2 = data.Ct22;
            sh.Range[index, 6].Value2 = data.Ct23;
            sh.Range[index, 7].Value2 = data.Ct25;
            sh.Range[index, 8].Value2 = data.Ct27;
            sh.Range[index, 9].Value2 = data.Ct30;
            sh.Range[index, 10].Value2 = data.Ct31;
            sh.Range[index, 11].Value2 = data.Ct32;
            sh.Range[index, 12].Value2 = data.Ct33;
            sh.Range[index, 13].Value2 = data.Ct36;
            sh.Range[index, 14].Value2 = data.Ct37;
            sh.Range[index, 16].Value2 = data.Ct38;
            sh.Range[index, 16].Value2 = data.Ct40;
            sh.Range[index, 17].Value2 = data.Ct41;
            sh.Range[index, 18].Value2 = data.Ct43;

            for (var i = 1; i <= headers.Length; i++)
            {
                sh.Range[index, i].BorderAround(LineStyleType.Thin);
            }

            for (var i = 5; i <= headers.Length; i++)
            {
                sh.Range[index, i].NumberFormat = "#,##0";
            }

            index++; //increment row index
            processedDocs++;
        }*/

        var fileName = $"{docList[0].Organization.TaxId}-01GTGT-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        using var stream = new MemoryStream();
        wb.SaveToStream(stream, FileFormat.Version2016);
        return (fileName, stream.ToArray());
    }

    #endregion

    /// <summary>
    /// Asynchronously processes a list of organizational documents and retrieves their associated payloads.
    /// </summary>
    /// <param name="docs">The collection of organizational documents to be processed.</param>
    /// <returns>
    /// A <see cref="Dictionary{OrgDocument, Document01GtgtPayload}"/> where each key is an <see cref="OrgDocument"/>
    /// and each value is the corresponding <see cref="Document01GtgtPayload"/>. If processing a document fails, it is excluded from the result.
    /// </returns>
    private async Task<Dictionary<OrgDocument, Document01GtgtPayload>> Get01GtgtPayloads(List<OrgDocument> docs)
    {
        Dictionary<OrgDocument, Document01GtgtPayload> payloads = [];
        List<Task<(OrgDocument doc, Document01GtgtPayload? payload)>> task = [];
        foreach (var doc in docs)
        {
            task.Add(Task.Run(async () => (doc, await Read_01GTGT_Document(doc))));
        }

        var result = await Task.WhenAll(task);
        foreach ((OrgDocument doc, Document01GtgtPayload? payload) in result)
        {
            if (payload is null)
            {
                logger.LogWarning("Failed to read document Id: [{id}].", doc.Id);
                continue;
            }
            payloads.Add(doc, payload);
        }

        return payloads;
    }

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
            stream.Position = 0; // Reset the stream's position back to start
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
            logger.LogError("Error while validating document. Caused by: {message}", e.Message);
            return (false, e.Message);
        }
    }

    private static async Task<string> Md5HashAsync(Stream stream)
    {
        using var md5 = MD5.Create();
        stream.Position = 0; // Reset position back to start
        var hashBytes = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}