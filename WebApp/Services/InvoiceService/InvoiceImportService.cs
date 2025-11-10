using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.FilterBuilder;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.UserService;

namespace WebApp.Services.InvoiceService;

public interface IInvoiceImportService
{
    Task<ResponseEntity> ImportPurchaseInvoice(List<IFormFile> files);
}

public class InvoiceImportService(IUserManager userManager,
                                  ILogger<InvoiceImportService> logger,
                                  IHostEnvironment env,
                                  IInvoiceMongoRepository mongoPurchaseInvoice,
                                  IAppRepository<Organization, Guid> orgRepository)
    : BaseAppService(userManager), IInvoiceImportService
{
    private const string UploadFolder = "Uploads";
    private const string PurchaseInvoiceFolder = "PurchaseInvoices";
    public async Task<ResponseEntity> ImportPurchaseInvoice(List<IFormFile> files)
    {
        if (!files.Any())
        {
            throw new EmptyInputException("No file uploaded");
        }
        
        var org = await orgRepository.Find(x => x.Id == WorkingOrg.ToGuid())
                                     .FirstOrDefaultAsync();
        if (org is null)
        {
            throw new NotFoundException("Organization not set");
        }
        
        var uploadPath = Path.Combine(env.ContentRootPath, UploadFolder, org.TaxId, PurchaseInvoiceFolder);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }
        
        List<InvoiceDetailDoc> docs = [];
        
        foreach (var file in files)
        {
            var xml = await XDocument.LoadAsync(file.OpenReadStream(), LoadOptions.None, CancellationToken.None);
            if (!ValidateXmlInvoice(xml))
            {
                continue; // skip invalid invoice
            }
            var sellerTaxId = xml.GetChildElementByPath("HDon/DLHDon/NDHDon/NBan/MST")?.Value;
            var buyerTaxId = xml.GetChildElementByPath("HDon/DLHDon/NDHDon/NMua/MST")?.Value;
            var invoiceGroupNotation = xml.GetChildElementByPath("HDon/DLHDon/TTChung/KHMSHDon")?.Value;
            var invoiceNotation = xml.GetChildElementByPath("HDon/DLHDon/TTChung/KHHDon")?.Value;
            var invoiceNumber = xml.GetChildElementByPath("HDon/DLHDon/TTChung/SHDon")?.Value;
            
            if (buyerTaxId != org.TaxId)
            {
                continue; // skip invalid invoice
            }
            
            docs.Add(MapToInvoiceDoc(xml));
            // save to disk
            
            var fileName = $"{sellerTaxId}_{invoiceGroupNotation}-{invoiceNotation}-{invoiceNumber}.xml";
            await using var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
            await file.CopyToAsync(fileStream);

            var filter = InvoiceFilterBuilder.StartBuilder()
                                             .WithBuyer(buyerTaxId)
                                             .WithSeller(sellerTaxId)
                                             .WithInvoiceNumber(invoiceNumber.ToInt())
                                             .WithKhhdon(invoiceNotation)
                                             .WithKhMshDon(invoiceGroupNotation.ToInt())
                                             .Build<InvoiceDetailDoc>();
            
            if (await mongoPurchaseInvoice.InvoiceExist(filter))
            {
                logger.LogWarning($"Duplicate invoice {fileName}");
                //continue; // skip duplicate invoice
            }

        }
        return ResponseEntity.OkResult(docs);
    }

    private InvoiceDetailDoc MapToInvoiceDoc(XDocument xml)
    {
        InvoiceDetailDoc doc = new()
        {
            Shdon = xml.GetChildElementByPath("HDon/DLHDon/TTChung/SHDon")?.Value.ToInt(),
            Khmshdon = xml.GetChildElementByPath("HDon/DLHDon/TTChung/KHMSHDon")?.Value.ToInt(),
            Khhdon = xml.GetChildElementByPath("HDon/DLHDon/TTChung/KHHDon")?.Value,
            Tdlap = xml.GetChildElementByPath("HDon/DLHDon/TTChung/NLap")?.Value.ToDateTime(),
            Nbmst = xml.GetChildElementByPath("HDon/DLHDon/NDHDon/NBan/MST")?.Value,
            Nbten = xml.GetChildElementByPath("HDon/DLHDon/NDHDon/NBan/Ten")?.Value,
            Nmmst = xml.GetChildElementByPath("HDon/DLHDon/NDHDon/NMua/MST")?.Value,
            Nmten = xml.GetChildElementByPath("HDon/DLHDon/NDHDon/NMua/Ten")?.Value,
            
            // TODO: map other fields,
        };

        return doc;
    }

    private bool ValidateXmlInvoice(XDocument xml)
    {
        return !string.IsNullOrEmpty(xml.GetChildElementByPath("HDon/DLHDon/NDHDon/NBan/MST")?.Value)
               && !string.IsNullOrEmpty(xml.GetChildElementByPath("HDon/DLHDon/TTChung/SHDon")?.Value);
    }
    
    
}