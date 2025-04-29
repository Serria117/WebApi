using Newtonsoft.Json;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Mongo.FilterBuilder;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Services.NotificationService;
using WebApp.Services.RestService;
using WebApp.Services.RestService.Dto.SoldInvoice;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services.InvoiceService;

/// <summary>
/// Interface for Sold Invoice Application Service
/// </summary>
public interface ISoldInvoiceAppService
{
    /// <summary>
    /// Retrieves sold invoices from the service within the specified date range.
    /// </summary>
    /// <param name="token">The authentication token used to access the service.</param>
    /// <param name="from">The start date of the range in which to retrieve sold invoices.</param>
    /// <param name="to">The end date of the range in which to retrieve sold invoices.</param>
    /// <returns>An <see cref="AppResponse"/> containing the result of the operation, including success status and retrieved data.</returns>
    Task<AppResponse> GetInvoiceFromService(string token, string from, string to);
}

/// <summary>
/// Implementation of Sold Invoice Application Service
/// </summary>
public class SoldInvoiceAppService(IUserManager userManager,
                                   ILogger<SoldInvoiceAppService> logger,
                                   IRestAppService restService,
                                   ISoldInvoiceDetailRepository soldInvoiceRepository,
                                   INotificationAppService notificationService) 
    : AppServiceBase(userManager), ISoldInvoiceAppService
{
    public async Task<AppResponse> GetInvoiceFromService(string token, string from, string to)
    {
        var response = await restService.GetSoldInvoiceInRangeAsync(token, from, to);
        
        if (response is not { Success: true, Data: not null })
        {
            logger.LogWarning("Invoice not found. {message}", response.Message);
            //await notificationService.SendAsync(UserId, HubName.InvoiceMessage, "No invoice found");
            return AppResponse.Error($"Invoice not found. {response.Message}");
        }

        var invoiceList = new List<SoldInvoiceModel>();
        var downloadCount = 0;
        var insertedCount = 0;
        var duplicatedCount = 0;
        
        foreach (var soldInvoice in (List<SoldInvoiceModel>) response.Data)
        {
            if(await IsDuplicate(soldInvoice)) //Check duplicate before add to list
            {
                await notificationService.SendAsync(UserId, HubName.InvoiceMessage,
                                                    $"Đã tồn tại hóa đơn số {soldInvoice.Shdon} trong hệ thống");
                continue; //Skip this invoice if it's already exist in database
            }
            invoiceList.Add(soldInvoice); //Add invoice to list if it's not duplicate
            duplicatedCount++; //Increment count of duplicated invoices
        }
        
        var total = invoiceList.Count;
        if (total == 0)
        {
            return AppResponse.SuccessResponse("No new invoices found");
        }
        var deserializedList = new List<SoldInvoiceDetail>();
        
        List<string> errors = [];
        // loop through each invoice model, call restService to extract detail information
        foreach (var invoice in invoiceList)
        {
            var invoiceDetailResponse = await restService.GetSoldInvoiceDetail(token, invoice); // Call RestService to get detail info
            //TODO: Handle response status not successsful
            if (invoiceDetailResponse is { Success: true, Data: SoldInvoiceDetail })
            {
                downloadCount++;
                await notificationService.SendAsync(UserId, HubName.InvoiceMessage, 
                                                    $"Tải thành công hóa đơn số {invoice.Shdon}. Đã tải {downloadCount}/{invoiceList.Count} hóa đơn");
                var invoiceDetailJson = JsonConvert.SerializeObject(invoiceDetailResponse.Data);
                try
                {
                    var invoiceDetail = JsonConvert.DeserializeObject<SoldInvoiceDetail>(invoiceDetailJson);
                    if (invoiceDetail != null)
                    {
                        // Process the deserialized data
                        deserializedList.Add(invoiceDetail);
                    }
                    else
                    {
                        logger.LogWarning("Failed to deserialize invoice {number} - result was null", invoice.Shdon);
                        await notificationService.SendAsync(UserId, HubName.InvoiceMessage, 
                                                            $"Không thể xử lý dữ liệu hóa đơn số {invoice.Shdon}");
                    }
                }
                catch (JsonReaderException ex)
                {
                    // Handles invalid JSON format
                    logger.LogError(ex, "Invalid JSON format for invoice {InvoiceNumber}", invoice.Shdon);
                    await notificationService.SendAsync(UserId, HubName.InvoiceMessage, 
                                                        $"Lỗi định dạng dữ liệu hóa đơn số {invoice.Shdon}");
                    errors.Add($"Hóa đơn số {invoice.Shdon}: Lỗi định dạng dữ liệu");
                }
                catch (JsonSerializationException ex)
                {
                    // Handles when JSON structure doesn't match the SoldInvoiceDetail class
                    logger.LogError(ex, "JSON structure mismatch for invoice {InvoiceNumber}", invoice.Shdon);
                    await notificationService.SendAsync(UserId, HubName.InvoiceMessage, 
                                                        $"Lỗi cấu trúc dữ liệu hóa đơn số {invoice.Shdon}");
                    errors.Add($"Hóa đơn số {invoice.Shdon}: Lỗi cấu trúc dữ liệu");
                }
                catch (Exception ex)
                {
                    // Handles any other unexpected exceptions
                    logger.LogError(ex, "Unexpected error deserializing invoice {InvoiceNumber}", invoice.Shdon);
                    await notificationService.SendAsync(UserId, HubName.InvoiceMessage, 
                                                        $"Lỗi không xác định khi xử lý hóa đơn số {invoice.Shdon}");
                    errors.Add($"Hóa đơn số {invoice.Shdon}: Lỗi không xác định");
                }

            }
            //In case of failure to deserialize json, use raw json string as input and try to deserialize again
            if (invoiceDetailResponse is { Success: true, Data: string })
            {
                var invoiceDetailJson = (string) invoiceDetailResponse.Data;
                try
                {
                    var invoiceDetail = JsonConvert.DeserializeObject<SoldInvoiceDetail>(invoiceDetailJson);
                    if (invoiceDetail != null)
                    {
                        // Process the deserialized data
                        deserializedList.Add(invoiceDetail);
                    }
                    else
                    {
                        logger.LogWarning("Failed to deserialize invoice {number} - result was null", invoice.Shdon);
                        await notificationService.SendAsync(UserId, HubName.InvoiceMessage,
                                                            $"Không thể xử lý dữ liệu hóa đơn số {invoice.Shdon}");
                        errors.Add($"Hóa đơn số {invoice.Shdon} ");
                    }
                }
                catch (JsonReaderException ex)
                {
                    // Handles invalid JSON format
                    logger.LogError(ex, "Invalid JSON format for invoice {InvoiceNumber}", invoice.Shdon);
                    await notificationService.SendAsync(UserId, HubName.InvoiceMessage,
                                                        $"Lỗi định dạng dữ liệu hóa đơn số {invoice.Shdon}");
                    errors.Add($"{invoice.Shdon}: lỗi định dạng dữ liệu hóa đơn");
                }
            }
            //Insert batch of 100 records at a time
            if (deserializedList.Count < 50)
            {
                continue;
            }

            insertedCount += await soldInvoiceRepository.InsertManyInvoiceAsync(deserializedList); //Insert many record at once
            deserializedList.Clear(); //Clear list after inserting
        }
        //Insert remaining records
        insertedCount += await soldInvoiceRepository.InsertManyInvoiceAsync(deserializedList);
        await notificationService.SendAsync(UserId, HubName.InvoiceMessage, 
                                            $"{insertedCount}/{total} hóa đơn đã được lưu.\n" +
                                            $"{duplicatedCount} hóa đơn đã có trong hệ thống.\n" +
                                            $"{errors.Count} hóa đơn lỗi.");
        return new AppResponse
        {
            Success = true,
            Message = "Ok",
            Data = new
            {
                Total = total,
                Inserted = insertedCount,
                Duplicated = duplicatedCount,
                ErrorCount = errors.Count,
                Errors = errors,
            }
        };
    }

    private async Task<bool> IsDuplicate(SoldInvoiceModel invoice)
    {
        var filter = InvoiceFilterBuilder.StartBuilder()
                                         .WithSeller(invoice.Nbmst)
                                         .WithInvoiceNumber(invoice.Shdon)
                                         .WithKhhdon(invoice.Khhdon)
                                         .WithKhMshDon(invoice.Khmshdon)
                                         .Build<SoldInvoiceDetail>();
        return await soldInvoiceRepository.InvoiceExist(filter);
    }
}