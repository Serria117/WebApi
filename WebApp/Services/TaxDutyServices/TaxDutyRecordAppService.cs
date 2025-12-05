using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities.Accounting.TaxDeclarations;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Services.Mappers;
using WebApp.Services.TaxDutyServices.Dto;
using WebApp.Services.UserService;
using WebApp.Utils;
using X.Extensions.PagedList.EF;
using X.PagedList.Extensions;

namespace WebApp.Services.TaxDutyServices;

public interface ITaxDutyRecordAppService
{
    Task<ResponseEntity> CreateDutyRecord(TaxDutyRecordDto input);
    Task<ResponseEntity> CreateDutyRecordsForOrganization(string period);
    Task<ResponseEntity> CreateDutyRecordsForOrganizations(TaxDutyRecordsCreateDto input);
    Task<ResponseEntity> UpdateTaxDutyRecord(string id, TaxDutyRecordDto input);
    Task<ResponseEntity> DeleteTaxDutyRecord(string id);

    Task<ResponseEntity> FindTaxDutyRecords(int? year,
                                            DutyPeriodType? periodType,
                                            string? period,
                                            DutyStatus? status,
                                            Guid? orgId,
                                            string? keyword);

    Task<ResponseEntity> UpdateTaxDutyRecords(ICollection<TaxDutyRecordUpdateDto> dtos);

    Task<ResponseEntity> UploadXmlToExistingRecord(TaxDutyRecordUploadXml input);

    //Task<(string FileName, byte[] File)> DownloadXmlDocument(string id);
    Task<ResponseEntity> RemoveXmlDocument(string docId, bool permanent = false);
    Task<ResponseEntity> ReplaceXmlInExistingRecord(string docId, TaxDutyRecordUploadXml input);
    Task<ResponseEntity> UpdateXmlStatus(TaxDutyDocumentUpdateStatus input);
    Task<ResponseEntity> GetDocumentsByRecord(string recordId);
    Task<(string FileName, byte[] File)> GetXmlContent(string docId);

    /// <summary>
    /// Uploads multiple XML files and processes them for tax duty records.
    /// </summary>
    /// <param name="uploadList">The object containing the collection of XML files to upload and a flag indicating whether to replace existing records.</param>
    /// <returns>A response entity indicating the success or failure of the operation, including any relevant data or messages.</returns>
    Task<ResponseEntity> UploadMultiXmlFiles(TaxDutyRecordUploadMultiXml uploadList);

    Task<ResponseEntity> GetDocumentTemplates();

    Task<ResponseEntity> GetDocumentByTaxId(string taxId, int fromYear, int toYear,
                                            string[] templateCode,
                                            int page = 1, int pageSize = 1000);
}

public class TaxDutyRecordAppService(IUserManager userManager,
                                     ILogger<TaxDutyRecordAppService> logger,
                                     AppDbContext dbContext) : BaseAppService(userManager), ITaxDutyRecordAppService
{
    public async Task<ResponseEntity> FindTaxDutyRecords(int? year,
                                                         DutyPeriodType? periodType,
                                                         string? period,
                                                         DutyStatus? status,
                                                         Guid? orgId,
                                                         string? keyword)
    {
        //Get all the organizations that belong to the current user.
        var organizationIds = await dbContext.Users
                                             .Where(u => u.Id == UserId.ToGuid())
                                             .SelectMany(u => u.Organizations.Select(o => o.Id))
                                             .ToHashSetAsync();
        keyword = keyword.RemoveSpace();
        var filterYear = year ?? DateTime.Now.Year; //force year to be current year if it is not provided
        var filterKeyword = keyword ?? string.Empty;
        var query = dbContext.TaxDutyRecords
                             .Where(x => x.Period.EndsWith(filterYear.ToString()))
                             .AsQueryable();

        if (status != null)
            query = query.Where(x => x.Status == status); //Filter with status
        if (orgId is not null && orgId != Guid.Empty)
            query = query.Where(x => x.OrganizationId == orgId); //Filter with organization
        if (periodType is not null)
            query = query.Where(x => x.DutyPeriodType == periodType);
        if (!string.IsNullOrEmpty(period))
            query = query.Where(x => x.Period == period); //Filter with period after period type filtering

        var duties = await query
                           .Include(x => x.XmlDocs)
                           .Join(dbContext.TaxReportDuties.Where(t => !t.Deleted),
                                 d => d.TaxDutyId, t => t.Id,
                                 (record, duty) => new
                                 {
                                     Record = record,
                                     Duty = duty
                                 })
                           .Join(dbContext.Organizations
                                          .Where(o => organizationIds.Contains(o.Id))
                                          .Where(o => o.UnsignName.Contains(filterKeyword)),
                                 rd => rd.Record.OrganizationId, o => o.Id,
                                 (rd, o) => new
                                 {
                                     DocumentCount = rd.Record.XmlDocs.Count(x => !x.Deleted),
                                     Report = rd.Duty,
                                     Duty = rd.Record,
                                     Org = o
                                 })
                           .AsNoTracking()
                           .AsSplitQuery()
                           .GroupBy(x => x.Org.Id)
                           .Select(g => new
                           {
                               OrganizationId = g.Key,
                               g.First().Org.FullName,
                               g.First().Org.ShortName,
                               g.First().Org.TaxId,
                               TotalTaxPayable = g.Sum(x => x.Duty.TaxPayable),
                               TaxDuties = g.Select(x => new
                               {
                                   x.Duty.Id,
                                   ReportName = x.Report.Name,
                                   ReportId = x.Report.Id,
                                   x.Duty.Period,
                                   x.Duty.Status,
                                   x.Duty.TaxPayable,
                                   x.Duty.DutyPeriodType,
                                   x.Duty.PaymentStatus,
                                   x.Duty.DueDate,
                                   x.DocumentCount
                               }).OrderBy(x => x.ReportId).ThenBy(x => x.DueDate).ToList()
                           }).ToListAsync();


        return ResponseEntity.OkResult(duties);
    }

    public async Task<ResponseEntity> UpdateTaxDutyRecords(ICollection<TaxDutyRecordUpdateDto> dtos)
    {
        var duties = await dbContext.TaxDutyRecords
                                    .Where(x => dtos.Select(d => d.Id).Contains(x.Id))
                                    .ToListAsync();
        if (duties.Count == 0) return ResponseEntity.Error404("No duty found");
        int unchangedCount = 0;
        foreach (var dto in dtos)
        {
            var duty = duties.FirstOrDefault(x => x.Id == dto.Id);
            if (duty is null) continue;

            //Remove from list if nothing changed
            if (duty.PaymentStatus == dto.PaymentStatus &&
                duty.TaxPayable == dto.TaxPayable &&
                dto.Status == duty.Status)
            {
                duties.Remove(duty);
                unchangedCount++;
                continue;
            }

            duty.PaymentStatus = dto.PaymentStatus;
            duty.TaxPayable = dto.TaxPayable;
            duty.Status = dto.Status;
        }

        await dbContext.BulkUpdateAsync(duties); //Bulk update duties
        return ResponseEntity.OkResult($"Cập nhật thành công {duties.Count}/{dtos.Count} bản ghi. " +
                                       $"{unchangedCount} bản ghi không có thay đổi.");
    }

    public async Task<ResponseEntity> CreateDutyRecord(TaxDutyRecordDto input)
    {
        var workingOrg = await dbContext.Organizations
                                        .CountAsync(x => x.Id == WorkingOrg.ToGuid());
        if (workingOrg <= 0) return ResponseEntity.Error404("Working organization not found");
        var taxDuty = await dbContext.TaxReportDuties
                                     .FirstOrDefaultAsync(x => x.Id == input.TaxDutyId);
        if (taxDuty is null) return ResponseEntity.Error404("Tax duty not found");
        var newRecord = new TaxDutyRecord
        {
            OrganizationId = WorkingOrg.ToGuid(),
            TaxDutyId = input.TaxDutyId,
            DueDate = input.DueDate,
            DutyPeriodType = input.DutyPeriodType,
            Period = input.Period,
            Status = input.Status,
            PaymentStatus = input.PaymentStatus,
            TaxPayable = input.TaxPayable,
            Note = input.Note
        };
        await dbContext.AddAsync(newRecord);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(newRecord);
    }

    public async Task<ResponseEntity> CreateDutyRecordsForOrganization(string period)
    {
        var duties = await dbContext.OrganizationTaxDuties
                                    .Where(x => x.OrganizationId == WorkingOrg.ToGuid())
                                    .ToListAsync();
        var periodValue = period.Split("-")[0].ToInt();
        var periodYear = period.Split("-")[1].ToInt();
        if (duties.Count == 0)
            return ResponseEntity.Error404("Current organization has no tax duty");
        List<TaxDutyRecord> records = [];

        records.AddRange(duties.Select(duty => new TaxDutyRecord()
        {
            OrganizationId = WorkingOrg.ToGuid(),
            TaxDutyId = duty.TaxReportDutyId,
            DutyPeriodType = duty.DutyPeriodType,
            DueDate = duty.DutyPeriodType switch
            {
                DutyPeriodType.Monthly => new DateTime(periodYear, periodValue, 1).AddMonths(1).AddDays(19),
                DutyPeriodType.Quarterly => StringExtension.GetQuarterEndDate(periodValue, periodYear),
                DutyPeriodType.Annual => new DateTime(periodYear, 12, 31).AddDays(90),
                _ => throw new NotImplementedException()
            },
            PaymentStatus = TaxPaymentStatus.Unpaid,
            Status = DutyStatus.Unreported,
            TaxPayable = 0,
            Period = period,
            Note = null
        }));

        await dbContext.AddRangeAsync(records);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> CreateDutyRecordsForOrganizations(TaxDutyRecordsCreateDto input)
    {
        var periodValue = 12;
        int periodYear;
        if (input.Period.Contains('/'))
        {
            var periodSplit = input.Period.Split('/');
            periodValue = periodSplit[0].ToInt();
            periodYear = periodSplit[1].ToInt();
        }
        else
        {
            periodYear = input.Period.ToInt();
        }

        var organizations = await dbContext.Users.Where(x => x.Id == UserId.ToGuid())
                                           .SelectMany(x => x.Organizations
                                                             .Select(o => o.Id))
                                           .Where(o => input.Organizations.Contains(o)) //filter only the selected ones
                                           .ToListAsync();
        List<TaxDutyRecord> records = [];
        foreach (var org in organizations)
        {
            var duties = await dbContext.OrganizationTaxDuties
                                        .Where(x => x.OrganizationId == org
                                                    && x.DutyPeriodType == input.DutyPeriodType
                                                    && !x.Deleted)
                                        .ToListAsync();
            if (duties.Count == 0) continue;
            var existingRecords = await dbContext.TaxDutyRecords
                                                 .AnyAsync(x => x.OrganizationId == org
                                                                && x.Period == input.Period
                                                                && !x.Deleted);
            if (existingRecords) continue;
            records.AddRange(duties.Select(duty => new TaxDutyRecord
            {
                OrganizationId = org,
                TaxDutyId = duty.TaxReportDutyId,
                DutyPeriodType = duty.DutyPeriodType,
                DueDate = duty.DutyPeriodType switch
                {
                    DutyPeriodType.Monthly => new DateTime(periodYear, periodValue, 1).AddMonths(1).AddDays(19),
                    DutyPeriodType.Quarterly => StringExtension.GetQuarterEndDate(periodValue, periodYear).AddMonths(1),
                    DutyPeriodType.Annual => new DateTime(periodYear, 12, 31).AddDays(90),
                    DutyPeriodType.Other => DateTime.Now.AddDays(10),
                    _ => throw new InvalidActionException("Invalid duty period type.")
                },
                PaymentStatus = TaxPaymentStatus.Unpaid,
                Status = DutyStatus.Unreported,
                TaxPayable = 0,
                Period = input.Period,
                Note = null
            }));
        }

        await dbContext.AddRangeAsync(records);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult($"Successfully created {records.Count} records.");
    }

    public async Task<ResponseEntity> UpdateTaxDutyRecord(string id, TaxDutyRecordDto input)
    {
        var found = await dbContext.TaxDutyRecords.FindAsync(id);
        if (found is null) return ResponseEntity.Error404("Tax duty record not found");
        found.Status = input.Status;
        found.PaymentStatus = input.PaymentStatus;
        found.TaxPayable = input.TaxPayable;
        found.Note = input.Note ?? found.Note;
        dbContext.UpdateRange(found);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(found);
    }

    public async Task<ResponseEntity> UploadXmlToExistingRecord(TaxDutyRecordUploadXml input)
    {
        var found = await dbContext.TaxDutyRecords
                                   .Where(r => !r.Deleted && r.Id == input.Id)
                                   .Select(r => new
                                   {
                                       r.Organization.TaxId,
                                       r.TaxDutyId
                                   })
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync();

        if (found is null) return ResponseEntity.Error404("Không tìm thấy bản ghi.");

        var uploadingXml = XDocument.Load(input.File.OpenReadStream());

        var mst = uploadingXml.Descendants()
                              .FirstOrDefault(n => n.Name.LocalName == "mst")?.Value ?? string.Empty;

        if (mst != found.TaxId)
            return ResponseEntity.Error400("Mã số thuế trên tờ khai không đúng.");

        var docCode = uploadingXml.Descendants()
                                  .FirstOrDefault(n => n.Name.LocalName == "maTKhai")?.Value ?? string.Empty;

        var template = await dbContext.TaxDeclarationTemplates
                                      .FirstOrDefaultAsync(t => t.TaxDutyId == found.TaxDutyId
                                                                && !t.Deleted
                                                                && t.Code == docCode);
        //Verify doc code
        if (template is null)
            return ResponseEntity.Error400("Mã tờ khai không đúng.");
        var mainContent = uploadingXml.Descendants()
                                      .FirstOrDefault(x => x.Name.LocalName == "CTieuTKhaiChinh");
        //Verify schema
        if (template.Schema is not null && mainContent is not null)
        {
            var schema = XElement.Parse(template.Schema);
            if (!schema.CompareStructure(mainContent))
            {
                return ResponseEntity.Error400("Tờ khai không đúng cấu trúc.");
            }
        }

        logger.LogInfoFormatted("Verify XML's schema successful. Creating new XML document.");
        //Extract other document's meta-data:
        var submissionCount = uploadingXml.Descendants()
                                          .FirstOrDefault(x => x.Name.LocalName == "soLan")?.Value.ToInt() ?? 0;
        var dateString = uploadingXml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ngayLapTKhai")?.Value;
        var issueDate =
            DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                                   out var date)
                ? date
                : (DateTime?)null;
        var period = uploadingXml.Descendants().FirstOrDefault(x => x.Name.LocalName == "kyKKhai")?.Value;
        var periodType = uploadingXml.Descendants().FirstOrDefault(x => x.Name.LocalName == "kieuKy")?.Value;

        //Check of the document is already uploaded by comparing the submission count and period.
        var existingDoc = await dbContext.TaxDutyXmlDocs
                                         .Where(t => t.TaxDutyRecordId == input.Id) //same record
                                         .Where(t => t.SubmissionCount == submissionCount) //same submission count
                                         .Where(t => t.Period == period) //same period
                                         .FirstOrDefaultAsync();

        if (existingDoc is not null)
        {
            return new ResponseEntity
            {
                Code = "409",
                Message = "Đã có tờ khai cùng kỳ kê khai, bạn có muốn thay thế tờ khai này không?",
                Success = false,
                Data = new
                {
                    ExistingId = existingDoc.Id
                }
            };
        }

        var xmlDocEntity = new TaxDutyXmlDoc
        {
            FileName = input.File.FileName,
            SubmissionCount = submissionCount,
            Content = uploadingXml.ToString(),
            TaxDutyRecordId = input.Id,
            TaxDeclarationTemplate = template,
            IssueDate = issueDate,
            Period = period,
            PeriodType = periodType,
            TaxId = found.TaxId,
        };
        dbContext.Add(xmlDocEntity);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok("Lưu dữ liệu XML thành công.");
    }

    public async Task<ResponseEntity> ReplaceXmlInExistingRecord(string docId, TaxDutyRecordUploadXml input)
    {
        var taxId = await dbContext.TaxDutyRecords.Where(r => !r.Deleted && r.Id == input.Id)
                                   .Select(r => r.Organization.TaxId)
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync();
        if (taxId == null)
            return ResponseEntity.Error404("Không tìm thấy bản ghi.");

        var uploadingXml = XDocument.Load(input.File.OpenReadStream());
        var documentTaxId = uploadingXml.Descendants().FirstOrDefault(x => x.Name.LocalName == "mst")?.Value;
        if (taxId != documentTaxId)
            return ResponseEntity.Error400("Mã số thuế trên tờ khai không đúng.");

        var xmlToReplace = await dbContext.TaxDutyXmlDocs
                                          .FirstOrDefaultAsync(t => t.Id == docId
                                                                    && t.TaxDutyRecordId == input.Id
                                                                    && !t.Deleted);

        if (xmlToReplace is null)
            return ResponseEntity.Error404("Không tìm thấy tài liệu phù hợp.");

        xmlToReplace.FileName = input.File.FileName;
        xmlToReplace.SubmissionCount = uploadingXml.Descendants()
                                                   .FirstOrDefault(x => x.Name.LocalName == "soLan")?.Value.ToInt() ??
                                       0;
        xmlToReplace.Content = uploadingXml.ToString(); //modyfy the content of the xml document

        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok("Thay thế file XML thành công!");
    }

    public async Task<ResponseEntity> UpdateXmlStatus(TaxDutyDocumentUpdateStatus input)
    {
        var found = await dbContext.TaxDutyXmlDocs.FirstOrDefaultAsync(t => t.Id == input.Id);

        if (found is null)
            return ResponseEntity.Error404("XML document not found");
        found.TaxResponseStatus = input.TaxResponseStatus;

        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok("Successfully updated XML document status.");
    }

    public async Task<ResponseEntity> RemoveXmlDocument(string docId, bool permanent = false)
    {
        var found = await dbContext.TaxDutyXmlDocs.FirstOrDefaultAsync(t => t.Id == docId);
        if (found is null) return ResponseEntity.Error404("XML document not found");
        if (permanent)
        {
            dbContext.TaxDutyXmlDocs.Remove(found); //hard delete
        }
        else
        {
            found.Deleted = true; //soft delete
        }

        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok("Successfully removed XML document.");
    }

    public async Task<ResponseEntity> DeleteTaxDutyRecord(string id)
    {
        var docs = await dbContext.TaxDutyXmlDocs.Where(x => x.TaxDutyRecordId == id)
                                  .Select(x => x.Id)
                                  .ToListAsync();
        if (docs.IsNotEmpty())
            await dbContext.TaxDutyXmlDocs
                           .DeleteRangeByKeyAsync(docs); //Delete all xml documents related to this record
        var result = await dbContext.TaxDutyRecords.DeleteByKeyAsync(id); //Delete the record itself
        return result > 0
            ? ResponseEntity.Ok("Successfully deleted tax duty record and all related documents.")
            : ResponseEntity.Error404("Tax duty record not found.");
    }

    public async Task<ResponseEntity> GetDocumentsByRecord(string recordId)
    {
        var docs = await dbContext.TaxDutyXmlDocs
                                  .Where(x => x.TaxDutyRecordId == recordId && !x.Deleted)
                                  .Select(d => new
                                  {
                                      d.Id,
                                      d.FileName,
                                      d.SubmissionCount,
                                      d.TaxResponseStatus,
                                      d.TaxTransactionCode,
                                      d.CreateAt,
                                      d.CreateBy,
                                      d.IssueDate,
                                      d.Period, d.PeriodType
                                  })
                                  .OrderBy(d => d.IssueDate)
                                  .ToListAsync();
        return ResponseEntity.OkResult(docs);
    }

    public async Task<ResponseEntity> GetDocumentTemplates()
    {
        var templates = await dbContext.TaxDeclarationTemplates.ToListAsync();
        return ResponseEntity.OkResult(templates.ProjectToDisplay([
            "Id", "Code", "Name", "Description", "TaxDutyId"
        ]));
    }

    public async Task<ResponseEntity> GetDocumentByTaxId(string taxId,
                                                         int fromYear, int toYear,
                                                         string[] templateCode,
                                                         int page = 1, int pageSize = 1000)
    {
        var query = dbContext.TaxDutyXmlDocs
                             .Where(x => x.TaxId == taxId)
                             .Where(x => x.Year >= fromYear && x.Year <= toYear);

        if (templateCode.Length > 0)
        {
            query = query.Where(x => x.TaxDeclarationTemplate.Code != null
                                     && templateCode.ToList().Contains(x.TaxDeclarationTemplate.Code));
        }

        var docs = await query.OrderBy(x => x.TemplateId)
                              .ThenBy(x => x.Period)
                              .ThenBy(x => x.SubmissionCount)
                              .Select(x => new XmlTaxDocumentWrapper(XDocument.Parse(x.Content)))
                              .ToPagedListAsync(page, pageSize);

        return ResponseEntity.OkResult(docs);
    }

    public async Task<(string FileName, byte[] File)> GetXmlContent(string docId)
    {
        var doc = await dbContext.TaxDutyXmlDocs
                                 .Where(x => x.Id == docId && !x.Deleted)
                                 .Select(x => new
                                 {
                                     x.FileName,
                                     x.Content
                                 })
                                 .FirstOrDefaultAsync();
        if (doc is null) throw new NotFoundException("Document not found");
        var xmlDoc = XDocument.Parse(doc.Content, LoadOptions.PreserveWhitespace);
        using var stream = new MemoryStream();
        xmlDoc.Save(stream);
        return (doc.FileName, stream.ToArray());
    }

    public async Task<ResponseEntity> UploadMultiXmlFiles(TaxDutyRecordUploadMultiXml uploadList)
    {
        const int maxFilesCount = 20;
        if (uploadList.Files.IsEmpty()) return ResponseEntity.Error400("Danh sách file trống.");
        if (uploadList.Files.Count > maxFilesCount)
            return ResponseEntity.Error400($"Danh sách file quá lớn, hãy giới hạn {maxFilesCount} file/lần");

        var dataToSave = new List<TaxDutyXmlDoc>();
        var dataToUpdate = new List<TaxDutyXmlDoc>();
        var errors = new List<string>();
        foreach (IFormFile file in uploadList.Files)
        {
            try
            {
                await using var stream = file.OpenReadStream();
                var xmlDoc = await XDocument.LoadAsync(stream, LoadOptions.PreserveWhitespace, CancellationToken.None);

                var doc = new XmlTaxDocumentWrapper(xmlDoc);
                var xmlRecord = doc.CreateXmlRecord();
                xmlRecord.FileName = file.FileName;

                var template = await dbContext.TaxDeclarationTemplates.FirstOrDefaultAsync(t => t.Code == doc.Code);
                if (template is not null) xmlRecord.TaxDeclarationTemplate = template;

                var record = await dbContext.TaxDutyRecords
                                            .Where(r => r.Organization.TaxId == doc.TaxId
                                                        && r.Period == doc.Period
                                                        && r.TaxDuty.Code == doc.Code)
                                            .Select(r => r.Id)
                                            .FirstOrDefaultAsync();
                if (record is not null) xmlRecord.TaxDutyRecordId = record;

                if (uploadList.ReplaceExisting)
                {
                    var existingDoc = await dbContext
                                            .TaxDutyXmlDocs
                                            .FirstOrDefaultAsync(t => t.TaxId == doc.TaxId
                                                                      && t.Period == doc.Period
                                                                      && t.SubmissionCount == doc.Count
                                                                      && t.TaxDeclarationTemplate.Code == doc.Code);
                    if (existingDoc is not null)
                    {
                        existingDoc.FileName = file.FileName;
                        existingDoc.Content = doc.Content;
                        existingDoc.IssueDate = doc.IssueDate.ToDateTime();
                        dataToUpdate.Add(existingDoc);
                    }
                    else
                    {
                        dataToSave.Add(xmlRecord);
                    }
                }
                else
                {
                    dataToSave.Add(xmlRecord);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                errors.Add(file.FileName);
            }
        }

        if (dataToSave.IsEmpty() && dataToUpdate.IsEmpty())
        {
            return ResponseEntity.Error400("Upload multi XML files failed.");
        }

        await dbContext.BulkInsertAsync(dataToSave);
        await dbContext.BulkUpdateAsync(dataToUpdate);

        return ResponseEntity.OkResult(new
        {
            UploadedCount = uploadList.Files.Count,
            SavedCount = dataToSave.Count + dataToUpdate.Count,
            FailedFiles = errors
        });
    }
}