using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Enums;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    public async Task<ResponseEntity> UploadExcelPayroll(ExcelPayrollDto input)
    {
        var orgId = WorkingOrg.ToGuid();
        var org = await dbContext.Organizations.FindAsync(orgId);
        if (org is null) throw new NotFoundException("Organization not found");
        var newId = Ulid.NewUlid().ToString();
        var fileName = Path.GetFileNameWithoutExtension(input.File.FileName);
        var extension = Path.GetExtension(input.File.FileName);
        var fileNameToSave = $"{newId}_{fileName}{extension}";
        var newExcelPayroll = new ExcelPayroll
        {
            Id = newId,
            OrganizationId = orgId,
            FileName = fileNameToSave,
            Year = input.Year,
            Version = input.Version
        };
        await dbContext.AddAsync(newExcelPayroll);
        await dbContext.SaveChangesAsync();

        var uploadDir = Path.Combine(env.ContentRootPath,
                                     FolderName.Uploads, FolderName.ExcelPayroll,
                                     orgId.ToString());
        
        if(!File.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
        
        var filePath = Path.Combine(env.ContentRootPath,
                                    FolderName.Uploads, FolderName.ExcelPayroll,
                                    orgId.ToString(),
                                    fileNameToSave);
        
        if(File.Exists(filePath)) File.Delete(filePath);
        
        await input.File.WriteToDiskAsync(filePath);

        return ResponseEntity.OkResult(newExcelPayroll);
    }

    public async Task<ResponseEntity> GetPayrollExcelList(int? year, string? keyword)
    {
        var found = await dbContext.ExcelPayrolls
                                   .Where(x => !x.Deleted)
                                   .Where(x => x.OrganizationId == WorkingOrg.ToGuid())
                                   .WhereIf(year.HasValue, x => x.Year == year)
                                   .WhereIf(string.IsNullOrWhiteSpace(keyword), x => x.FileName.Contains(keyword!))
                                   .Select(x => new ExcelPayrollDisplayDto()
                                   {
                                       Id = x.Id,
                                       FileName = x.FileName,
                                       Year = x.Year,
                                       Version = x.Version,
                                       IsFinal = x.IsFinal
                                   })
                                   .ToListAsync();

        return ResponseEntity.OkResult(found);
    }

    public async Task<(string FileName, byte[] File)> DownloadExcelPayroll(string id)
    {
        var found = await dbContext.ExcelPayrolls
                                   .Where(x => x.Id == id
                                               && x.OrganizationId == WorkingOrg.ToGuid())
                                   .FirstOrDefaultAsync()
                                   ?? throw new NotFoundException("Không tìm thấy bản ghi.");
        string filePath = Path.Combine(env.ContentRootPath, 
                                       FolderName.Uploads,
                                       found.OrganizationId.ToString(),
                                       found.FileName);

        var byteArray = await filePath.ReadToBytesAsync();

        return (found.FileName, byteArray);
    }

    public async Task<ResponseEntity> UpdateExcelPayrollStatus(string id)
    {
        var found = await dbContext.ExcelPayrolls
                                   .Where(x => x.Id == id
                                               && x.OrganizationId == WorkingOrg.ToGuid())
                                   .FirstOrDefaultAsync() 
                                   ?? throw new NotFoundException("Không tìm thấy bản ghi.");
        found.IsFinal = !found.IsFinal;
        dbContext.ExcelPayrolls.Update(found);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(found);
    }

    public async Task<ResponseEntity> UpdateExcelPayroll(ExcelPayrollUpdateDto input)
    {
        var found = await dbContext.ExcelPayrolls
                                   .Where(x => x.Id == input.Id
                                               && x.OrganizationId == WorkingOrg.ToGuid())
                                   .FirstOrDefaultAsync();
        if (found is null) throw new NotFoundException("Không tìm thấy bản ghi.");
        string filePath = Path.Combine(env.ContentRootPath,
                                       FolderName.Uploads,
                                       found.OrganizationId.ToString(),
                                       found.FileName);
        if (File.Exists(filePath)) File.Delete(filePath);
        await input.File.WriteToDiskAsync(filePath);
        return ResponseEntity.OkResult("Cập nhật file thành công.");
    }

    public async Task<ResponseEntity> DeleteExcelPayroll(string id)
    {
        var found = await dbContext.ExcelPayrolls
                                   .Where(x => x.Id == id
                                               && x.OrganizationId == WorkingOrg.ToGuid())
                                   .FirstOrDefaultAsync();
        if (found is null) throw new NotFoundException("Không tìm thấy bản ghi.");
        string filePath = Path.Combine(env.ContentRootPath,
                                       FolderName.Uploads,
                                       found.OrganizationId.ToString(),
                                       found.FileName);
        if (File.Exists(filePath)) File.Delete(filePath);
        dbContext.ExcelPayrolls.Remove(found);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult("Xóa file thành công.");
    }
}