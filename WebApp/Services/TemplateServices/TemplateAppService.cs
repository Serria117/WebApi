using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.Mappers;
using WebApp.Services.TemplateServices.Dto;
using WebApp.Utils;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.TemplateServices;

public interface ITemplateAppService
{
    Task<ResponseEntity> CreateTemplate(TemplateCreateDto dto);
    Task DeleteTemplateFile(int fileId);
    Task<(string FileName, byte[] FileData)> DownloadFile(int fileId);
    Task<ResponseEntity> FindTemplates(PageRequest request);
    Task<ResponseEntity> GetTemplateById(int id);
    Task<ResponseEntity> UpdateTemplateInfo(int id, TemplateCreateDto dto);
    Task<ResponseEntity> UploadNewFileToTemplate(TemplateFileUploadDto dto);
}

public class TemplateAppService(IAppRepository<Template, int> templateRepository,
                                IAppRepository<TemplateFile, int> fileRepository,
                                IHostEnvironment env) : ITemplateAppService
{
    private const string TemplateFolder = "DocumentTemplates";

    public async Task<ResponseEntity> CreateTemplate(TemplateCreateDto dto)
    {
        var newTemplate = dto.ToEntity();

        if (dto.TemplateFiles.Count > 0)
        {
            foreach (var file in dto.TemplateFiles)
            {
                if (file.FileToUpload is null) continue;
                var fileName = await UploadTemplateFile(file.FileToUpload);
                newTemplate.TemplateFiles.Add(new TemplateFile
                {
                    FileName = fileName,
                    FilePath = $@"Uploads\{TemplateFolder}\{fileName}",
                    Version = file.Version,
                    VersionNote = file.VersionNote,
                    UploadTime = DateTime.Now.ToLocalTime()
                });
            }
        }

        var createdTemplate = await templateRepository.CreateAsync(newTemplate);
        return ResponseEntity.OkResult(createdTemplate.ToDisplayDto());
    }

    public async Task<ResponseEntity> FindTemplates(PageRequest request)
    {
        var found = await templateRepository.Find(t => !t.Deleted)
                                            .WhereIf(request.Keyword is not null,
                                                     t => t.Name.Contains(request.Keyword!))
                                            .OrderBy(t => t.Order)
                                            .ToPagedListAsync(request.Page, request.Size);
        return ResponseEntity.OkResult(found.MapPagedList(t => t.ToDisplayDto()));
    }

    public async Task<ResponseEntity> GetTemplateById(int id)
    {
        var found = await templateRepository.Find(t => !t.Deleted && t.Id == id)
                                            .Include(t => t.TemplateFiles
                                                           .Where(x => !x.Deleted)
                                                           .OrderByDescending(x => x.UploadTime))
                                            .FirstOrDefaultAsync();
        if (found == null) return ResponseEntity.Error404("Template not found");
        var dto = found.ToDisplayDto();
        return ResponseEntity.OkResult(dto);
    }

    public async Task<ResponseEntity> UpdateTemplateInfo(int id, TemplateCreateDto dto)
    {
        var template = await templateRepository.Find(t => !t.Deleted && t.Id == id)
                                               .FirstOrDefaultAsync();
        if (template is null) return ResponseEntity.Error404("Template not found");

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.Order = dto.Order;

        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> UploadNewFileToTemplate(TemplateFileUploadDto dto)
    {
        if (dto.TemplateFile.FileToUpload is null) return ResponseEntity.Error400("File is empty");
        var template = await templateRepository.Find(x => x.Id == dto.TemplateId && !x.Deleted)
                                               .FirstOrDefaultAsync();

        if (template is null) return ResponseEntity.Error404("Template not found");
        var fileName = await UploadTemplateFile(dto.TemplateFile.FileToUpload);
        template.TemplateFiles.Add(new TemplateFile
        {
            FileName = fileName,
            FilePath = $"Uploads\\{TemplateFolder}\\{fileName}",
            Version = dto.TemplateFile.Version,
            VersionNote = dto.TemplateFile.VersionNote,
            UploadTime = DateTime.Now.ToLocalTime()
        });
        await templateRepository.UpdateAsync(template);
        return ResponseEntity.Ok();
    }

    public async Task<(string FileName, byte[] FileData)> DownloadFile(int fileId)
    {
        var file = await fileRepository.Find(x => x.Id == fileId).FirstOrDefaultAsync();
        if (file == null) throw new KeyNotFoundException("Id not found.");
        var filePath = Path.Combine(env.ContentRootPath, file.FilePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("The file has been moved or deleted from disk.");

        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            await fileStream.CopyToAsync(memoryStream);
        }

        memoryStream.Position = 0;
        return (file.FileName, memoryStream.ToArray());
    }

    public async Task DeleteTemplateFile(int fileId)
    {
        var file = await fileRepository.Find(x => x.Id == fileId).FirstOrDefaultAsync()
                   ?? throw new KeyNotFoundException("Id not found.");
        var filePath = Path.Combine(env.ContentRootPath, file.FilePath);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        await fileRepository.SoftDeleteAsync(fileId);
    }

    private async Task<string> UploadTemplateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        var contentRoot = env.ContentRootPath;
        var templatesDir = Path.Combine(contentRoot, "Uploads", TemplateFolder);

        if (!Directory.Exists(templatesDir))
            Directory.CreateDirectory(templatesDir);

        var fileName = Path.GetFileNameWithoutExtension(file.FileName)
                       + Guid.NewGuid() + "." + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(templatesDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }
}