using System.Runtime.InteropServices;
using System.Text.Json;
using Newtonsoft.Json;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Enums;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.FilterBuilder;
using WebApp.Payloads;
using WebApp.Payloads.Messages;
using WebApp.Services.CommonService;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService.Dto;
using WebApp.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace WebApp.Services.InvoiceService;

public partial class InvoiceService
{
    public async Task<ResponseEntity> ExtractPurchaseInvoices(string token,
                                                              string from, string to,
                                                              int[]? invoiceTypes)
    {
        var taxId = TokenHandler.ReadJwtToken(token).Subject;
        logger.LogInfoFormatted($"[{taxId}] extracting purchase invoice's details " +
                                $"from {from} to {to}");
        var result = await restService.GetPurchaseInvoiceListInRange(token, from, to, invoiceTypes);
        var countFromResponse = result.TotalCount ?? 0L;
        if (countFromResponse == 0L)
        {
            logger.LogInfoFormatted($"[{taxId}] no purchase invoice found " +
                                    $"from {from} to {to}");
            await invoiceHistoryAppService.CreateHistoryAsync(from.ToDateTime()!.Value,
                                                              to.ToDateTime()!.Value,
                                                              0, 0, SyncType.Purchased);

            return new ResponseEntity
            {
                Message = $"Không có hóa đơn mới nào trong khoảng thời gian từ {from} đến {to}",
                Code = "200"
            };
        }

        if (result is not { Success: true, Data: not null })
        {
            logger.LogWarning("Invoice retrieving operation failed. {message}", result.Message);
            await invoiceHistoryAppService.CreateHistoryAsync(from.ToDateTime()!.Value,
                                                              to.ToDateTime()!.Value,
                                                              totalFound: 0, totalSuccess: 0,
                                                              SyncType.Purchased, success: false);
            return new ResponseEntity
            {
                Message = "Failed to retrieve purchase invoices. " +
                          $"Please try again later. {result.Message}",
                Code = "400",
                Success = false
            };
        }

        var existingCount = 0;
        var downloadCount = 0;
        var insertedCount = 0;
        var errorCount = 0;
        var errorMessages = new List<string>();
        var errorInvoiceList = new List<SoldInvoiceErrorDisplay>();

        var responseInvoiceList = (List<InvoiceModel>)result.Data;

        List<InvoiceDetailModel> deSerializableInvoices = [];

        //invoices that cannot be deserialized will be stored as string
        //and attempt to deserialize using JsonSerializer
        List<string> unDeserializableInvoices = [];

        var invoicesToSaveList = new List<InvoiceModel>();

        //Begin getting invoice detail, first check if invoice already exists:
        foreach (var invoice in responseInvoiceList)
        {
            if (await IsPurchaseInvoiceExist(invoice))
            {
                existingCount++;
                Console.WriteLine($"Inv no: [{invoice.Shdon}-{invoice.Khhdon}] already existed in database. Count={existingCount}");
                continue;
            };
            invoicesToSaveList.Add(invoice); //if not exist, add to the list
        }

        if (invoicesToSaveList.Count == 0)
            return new ResponseEntity()
            {
                Message = $"Tìm thấy {existingCount} hóa đơn mua hàng đã có trong hệ thống, " +
                          "không có hóa đơn mới.",
                Code = "200",
                Success = true
            };

        foreach (var invoice in invoicesToSaveList)
        {
            ResponseEntity invDetailResponse = await restService.GetPurchaseInvoiceDetail(token, invoice);

            if (invDetailResponse.Code == nameof(InvoiceDetailStatus.TooManyRequest))
            {
                logger.LogWarning("Rate limit exceeded while fetching invoice details. " +
                                  "Please try again later.");
                var deserializableResult = await WriteDeserializableInvoices(deSerializableInvoices);
                var undeserializableResult = await WriteUndeserializableInvoices(unDeserializableInvoices);
                return new ResponseEntity
                {
                    Success = true,
                    Message = invDetailResponse.Message,
                    Code = invDetailResponse.Code,
                    Data = new
                    {
                        Total = countFromResponse,
                        Existing = existingCount,
                        Success = deSerializableInvoices.Count + unDeserializableInvoices.Count
                                  - deserializableResult.ErrorCount - undeserializableResult.ErrorCount,
                        Remaining = deserializableResult.ErrorCount + undeserializableResult.ErrorCount,
                    }
                };
            }

            if (invDetailResponse is not { Success: true, Data: not null })
            {
                logger.LogWarning("Failed to retrieve invoice detail for invoice {invoiceNumber}. " +
                                  "Error: {message}", invoice.Shdon, invDetailResponse.Message);
                continue;
            }

            if (invDetailResponse.Code == InvoiceDetailStatus.Success.ToString())
            {
                downloadCount++;
                deSerializableInvoices.Add((InvoiceDetailModel)invDetailResponse.Data);
                await notificationService.SendAsync(UserId,
                                                    HubName.InvoiceStatus,
                                                    InvoiceMessage.Create(
                                                        saved: downloadCount, total: invoicesToSaveList.Count));
                continue;
            }

            if (invDetailResponse.Code == InvoiceDetailStatus.Undeserializable.ToString())
            {
                downloadCount++;
                unDeserializableInvoices.Add(JsonConvert.SerializeObject(invDetailResponse.Data));
                await notificationService.SendAsync(UserId,
                                                    HubName.InvoiceStatus,
                                                    InvoiceMessage.Create(
                                                        saved: downloadCount, total: invoicesToSaveList.Count));
            }
        }

        logger.LogInformation($"Deserializable invoices count = {deSerializableInvoices.Count}");
        logger.LogInformation($"Undeserializable invoices count = {unDeserializableInvoices.Count}");

        var dResult = await WriteDeserializableInvoices(deSerializableInvoices);
        var uResult = await WriteUndeserializableInvoices(unDeserializableInvoices);
        errorCount = dResult.ErrorCount + uResult.ErrorCount;
        //Write history:
        await invoiceHistoryAppService.CreateHistoryAsync(
            from: from.ToDateTime()!.Value,
            to: to.ToDateTime()!.Value,
            totalFound: countFromResponse,
            totalSuccess: downloadCount - dResult.ErrorCount - uResult.ErrorCount,
            type: SyncType.Purchased);

        return new ResponseEntity
        {
            TotalCount = countFromResponse,
            Code = "200",
            Success = true,
            Message = $"Tìm thấy {existingCount}/{countFromResponse} hóa đơn đã có trong hệ thống. " +
                      $"Đã thêm {downloadCount}/{invoicesToSaveList.Count} hóa đơn mua hàng mới vào hệ thống.",
            Data = new
            {
                Total = countFromResponse,
                Existing = existingCount,
                Inserted = downloadCount - errorCount,
                ErrorCount = errorCount,
                Errors = dResult.Error.Union(uResult.Error).ToList(),
            }
        };
    }

    public async Task<ResponseEntity> QueryPurchaseInvoices(string taxCode, InvoiceRequestParam invoiceParams)
    {
        var filter = InvoiceFilterBuilder.StartBuilder()
                                         .FromDate(invoiceParams.From)
                                         .ToDate(invoiceParams.To)
                                         .WithBuyer(taxCode)
                                         .WithInvoiceNumber(invoiceParams.InvoiceNumber)
                                         .HasNameKeyword(invoiceParams.NameKeyword)
                                         .WithRisk(invoiceParams.Risk)
                                         .WithStatus(invoiceParams.Status)
                                         .WithType(invoiceParams.InvoiceType)
                                         .Build<InvoiceDetailDoc>();

        var invoiceList = await mongoPurchaseInvoice
            .FindInvoices(filter, invoiceParams.Page ?? 1, invoiceParams.Size ?? 10);

        var data = new List<InvoiceDisplayDto>();
        // Convert each invoice to display model, catch any conversion errors and log them
        foreach (var inv in invoiceList.Data)
        {
            InvoiceDisplayDto displayModel;
            try
            {
                displayModel = inv.ToDisplayModel();
            }
            catch (Exception ex)
            {
                logger.LogErrorFormatted(exception: ex,
                                         message: $"Error converting invoice {inv.Id} to display model.");
                displayModel = new InvoiceDisplayDto
                {
                    InvoiceNumber = inv.Shdon?.ToString(),
                    SellerName = "Lỗi khi chuyển đổi, kiểm tra lại dữ liệu hóa đơn",
                    SellerTaxCode = inv.Nbmst ?? string.Empty,
                    CreationDate = inv.Tdlap?.ToLocalTime()
                };
            }

            data.Add(displayModel);
        }

        return new ResponseEntity
        {
            Data = data,
            Message = "Ok",
            TotalCount = invoiceList.Total,
            PageNumber = invoiceParams.Page,
            PageSize = invoiceParams.Size,
            Success = true,
            PageCount = invoiceList.PageCount
        };
    }

    public async Task<ResponseEntity> UploadPurchaseInvoices(List<IFormFile> files)
    {
        try
        {
            List<InvoiceDetailDoc> invoices = [];
            foreach (IFormFile file in files)
            {
                InvoiceDetailDoc invoice = await ReadInvoiceFromXml(file);
                if (await IsPurchaseInvoiceExist(invoice)) continue;
                invoices.Add(invoice);
            }

            var result = await mongoPurchaseInvoice.InsertInvoicesAsync(invoices);
            return new ResponseEntity
            {
                Success = result,
                Code = result ? "200" : "400",
                Data = invoices.Select(i => i.ToDisplayModel()).ToList()
            };
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return ResponseEntity.Error400("Tải lên file không thành công. Hãy kiểm tra log.");
        }
    }

    public async Task<ResponseEntity> DeletePurchaseInvoicesAsync(List<string> ids)
    {
        var result = await mongoPurchaseInvoice.DeleteInvoices(ids);
        return new ResponseEntity
        {
            Success = result,
            Code = result ? "200" : "400",
            Message = result ? "Xóa hóa đơn thành công" : "Xóa hóa đơn không thành công"
        };
    }

    public async Task<ResponseEntity> RecheckPurchaseInvoiceStatus(string token, string from, string to)
    {
        var result = await restService.GetPurchaseInvoiceListInRange(token, from, to);
        var total = 0L;
        List<InvoiceDisplayDto> updateList = [];
        if (result.Success && result.Data is List<InvoiceModel> invoiceList)
        {
            foreach (var inv in invoiceList)
            {
                var updateResult = await mongoPurchaseInvoice.UpdateInvoiceStatus(inv.Id!, inv.Tthai!.Value);
                if (updateResult <= 0) continue;
                total += updateResult;
                updateList.Add(inv.ToDisplayModel());
            }
        }

        return new ResponseEntity
        {
            Message = total > 0
                ? $"{total:N0} hóa đơn đã được cập nhật trạng thái"
                : "Không có hóa đơn cần cập nhật trạng thái",
            Data = updateList,
        };
    }

    private async Task<bool> IsPurchaseInvoiceExist(InvoiceModel invoice)
    {
        var filter = InvoiceFilterBuilder.StartBuilder()
                                         .WithBuyer(invoice.Nmmst)
                                         .WithKhhdon(invoice.Khhdon)
                                         .WithInvoiceNumber(invoice.Shdon)
                                         .Build<InvoiceDetailDoc>();
        return await mongoPurchaseInvoice.InvoiceExist(filter);
    }

    private async Task<bool> IsPurchaseInvoiceExist(InvoiceDetailDoc invoice)
    {
        var filter = InvoiceFilterBuilder.StartBuilder()
                                         .WithBuyer(invoice.Nmmst)
                                         .WithKhhdon(invoice.Khhdon)
                                         .WithInvoiceNumber(invoice.Shdon)
                                         .Build<InvoiceDetailDoc>();
        return await mongoPurchaseInvoice.InvoiceExist(filter);
    }
}