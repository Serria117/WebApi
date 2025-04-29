using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Spire.Xls;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Enums;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Mongo.FilterBuilder;
using WebApp.Mongo.Mapper;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.NotificationService;
using WebApp.Services.RestService;
using WebApp.Services.RestService.Dto;
using WebApp.Services.RestService.Dto.SoldInvoice;
using WebApp.Services.RiskCompanyService;
using WebApp.Services.UserService;
using WebApp.SignalrConfig;
using WebApp.Utils;

namespace WebApp.Services.InvoiceService;

public interface IInvoiceAppService
{
    /// <summary>
    /// Find invoices by organization and query parameters
    /// </summary>
    /// <param name="taxCode"></param>
    /// <param name="invoiceParams"></param>
    /// <returns>The invoice list</returns>
    Task<AppResponse> FindPurchaseInvoices(string taxCode, InvoiceRequestParam invoiceParams);

    /// <summary>
    /// Sync invoices from hoadondientu.gdt.gov.vn
    /// </summary>
    /// <param name="token">The access token from hoadondientu.gdt.gov.vn</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>Success result if all invoices were synced</returns>
    Task<AppResponse> ExtractPurchaseInvoices(string token, string from, string to);


    /// <summary>
    /// Export invoice list to excel file
    /// </summary>
    /// <param name="taxCode">Organization taxcode</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>The byte array of created excel file to download</returns>
    Task<byte[]?> ExportExcel(string taxCode, string from, string to);

    /// <summary>
    /// Recheck the saved invoices in the database and attempt to update their status if any change.
    /// </summary>
    /// <param name="token">The access token from hoadondientu.gdt.gov.vn</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>The result of checking process.</returns>
    Task<AppResponse> RecheckPurchaseInvoice(string token, string from, string to);

    Task<AppResponse> FindOne(string taxCode, string id);
    Task<AppResponse> ExtractSoldInvoice(SyncInvoiceRequest request);
    Task<AppResponse> FindSoldInvoices(string taxCode, InvoiceRequestParam invoiceParams);
}

public class InvoiceAppService(IInvoiceMongoRepository mongoPurchaseInvoice,
                               ISoldInvoiceMongoRepository mongoSoldInvoice,
                               IRestAppService restService,
                               ILogger<InvoiceAppService> logger,
                               IRiskCompanyAppService riskService,
                               IAppRepository<SyncInvoiceHistory, long> historyRepository,
                               ISoldInvoiceDetailRepository soldInvoiceDetailRepository,
                               INotificationAppService notificationService,
                               IUserManager userManager) : AppServiceBase(userManager), IInvoiceAppService
{
    #region Sold Invoices

    public async Task<AppResponse> ExtractSoldInvoice(SyncInvoiceRequest request)
    {
        var result = await restService.GetSoldInvoiceInRangeAsync(request.Token, request.From, request.To);
        var total = 0;
        var inserted = 0;
        if (result.Data is List<SoldInvoiceModel> invoices)
        {
            total = invoices.Count;
            var docs = invoices.Select(x => JsonConvert.SerializeObject(x).ToSoldInvoiceBson()).ToList();
            inserted = await mongoSoldInvoice.InsertInvoicesAsync(docs);
        }

        return new AppResponse
        {
            Success = true,
            Code = "200",
            Data = new
            {
                Total = total,
                Inserted = inserted,
            },
            Message = $"Inserted {inserted} of {total}",
        };
    }

    public async Task<AppResponse> FindSoldInvoices(string taxCode, InvoiceRequestParam invoiceParams)
    {
        invoiceParams.Valid();
        //filter by seller taxid
        var filter = InvoiceFilterBuilder.StartBuilder()
                                         .WithSeller(taxCode)
                                         .HasNameKeyword(invoiceParams.NameKeyword)
                                         .FromDate(invoiceParams.From)
                                         .ToDate(invoiceParams.To)
                                         .WithInvoiceNumber(invoiceParams.InvoiceNumber)
                                         .Build<SoldInvoiceDetail>();

        var result = await soldInvoiceDetailRepository.FindInvoiceAsync(filter,
                                                                        invoiceParams.Page!.Value,
                                                                        invoiceParams.Size!.Value);

        return new AppResponse
        {
            Success = true,
            Data = result.Data.Select(x => x.ToDisplayModel()).ToList(),
            Code = "200",
            PageCount = result.PageCount,
            PageNumber = result.Page,
            PageSize = result.Size,
            TotalCount = result.Total,
            Message = "Ok"
        };
    }

    #endregion

    #region Purchase Invoice

    public async Task<AppResponse> FindPurchaseInvoices(string taxCode, InvoiceRequestParam invoiceParams)
    {
        invoiceParams.Valid();

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
        var invoiceList = await mongoPurchaseInvoice.FindInvoices(filter: filter,
                                                                  page: invoiceParams.Page!.Value,
                                                                  size: invoiceParams.Size!.Value);
        await notificationService.SendAsync(UserId,
                                            HubName.InvoiceMessage,
                                            $"Found {invoiceList.Total} invoice(s)");
        return new AppResponse
        {
            Data = invoiceList.Data.Select(inv => inv.ToDisplayModel()).ToList(),
            Message = "Ok",
            TotalCount = invoiceList.Total,
            PageNumber = invoiceParams.Page,
            PageSize = invoiceParams.Size,
            Success = true,
            PageCount = invoiceList.PageCount
        };
    }

    public async Task<AppResponse> RecheckPurchaseInvoice(string token, string from, string to)
    {
        var resultFromService = await restService.GetPurchaseInvoiceListInRange(token, from, to);
        var total = 0L;
        List<InvoiceDisplayDto> updateList = [];
        if (resultFromService is { Success: true, Data: List<InvoiceModel> invoiceList })
        {
            foreach (var inv in invoiceList)
            {
                var result = await mongoPurchaseInvoice.UpdateInvoiceStatus(inv.Id!, inv.Tthai!.Value);
                if (result <= 0) continue;
                total += result;
                updateList.Add(inv.ToDisplayModel());
            }
        }

        return new AppResponse
        {
            Message = total > 0
                ? $"{total:N0} hóa đơn đã được cập nhật trạng thái"
                : "Không có hóa đơn cần cập nhật trạng thái",
            Data = updateList,
        };
    }

    //TODO: Refactor ExtractPurchaseInvoices method
    public async Task<AppResponse> ExtractPurchaseInvoices(string token, string from, string to)
    {
        logger.LogInformation("Sync Invoices from {from} to {to} at {time}",
                              from, to, DateTime.Now.ToLocalTime());
        var result = await restService.GetPurchaseInvoiceListInRange(token, from, to);

        if (result is not { Success: true, Data: not null })
        {
            logger.LogWarning("Invoice not found. {message}", result.Message);
            return AppResponse.Error("Invoice not found");
        }

        var invoiceList = (List<InvoiceModel>)result.Data;
        var totalFound = invoiceList.Count;
        if (totalFound == 0)
        {
            await notificationService.SendAsync(UserId, HubName.InvoiceMessage,
                                                $"Không có hóa đơn phát sinh từ {from} dến {to}!");
            return AppResponse.SuccessResponse("No new invoices found");
        }

        var buyerTaxId = invoiceList.First().Nmmst;
        List<InvoiceDetailModel> deSerializedInvoices = [];
        List<string> unDeserializedInvoices = [];
        var countAdd = 1;

        var newInvoices = new List<InvoiceModel>();

        //Keep only those which are not duplicated
        foreach (InvoiceModel inv in invoiceList)
        {
            if (await IsPurchaseInvoiceDuplicate(inv)) continue;
            newInvoices.Add(inv);
        }

        await notificationService.SendAsync(UserId, HubName.InvoiceMessage, "Starting sync...");
        
        foreach (var invoice in newInvoices)
        {
            var invDetail = await restService.GetPurchaseInvoiceDetail(token, invoice);

            //If code 419 is hit, write anything that has already been retrieved and stop
            if (invDetail.Code == "429")
            {
                logger.LogWarning("Server has reach rate limit. Writing {retrieved}/{total} invoices to database",
                                  unDeserializedInvoices.Count + deSerializedInvoices.Count,
                                  invoiceList.Count);
                await notificationService
                    .SendAsync(UserId,
                               HubName.InvoiceMessage,
                               "Some invoices could not be synced right now " +
                               "because the external server has hit rate limit.");

                return await WriteInvoices(deSerializedInvoices, unDeserializedInvoices, newInvoices.Count);
            }

            if (invDetail is not { Success: true })
            {
                logger.LogWarning("Error: {message}", invDetail.Message);
                logger.LogInformation("Skipping...\n {data}", invoice.Shdon);
                await notificationService
                    .SendAsync(UserId,
                               HubName.InvoiceMessage,
                               $"Failed to save invoice {invoice.Shdon} of {invoice.Nbmst}, " +
                               $"created at: {invoice.Tdlap:dd/MM/yyyy}");
                continue;
            }

            //Handle successful deserializion
            if (invDetail is { Success: true, Data: InvoiceDetailModel invoiceToAdd })
            {
                invoiceToAdd.Risk = riskService.IsInvoiceRisk(invoiceToAdd.Nbmst);
                deSerializedInvoices.Add(invoiceToAdd);
                logger.LogInformation(
                    "{count}/{new} - Invoice {invNum} added to collection.",
                    countAdd, newInvoices.Count, invoiceToAdd.Shdon
                );
                var completed = decimal.Divide(countAdd, newInvoices.Count) * 100;
                await notificationService.SendAsync(UserId,
                                                    HubName.InvoiceMessage,
                                                    $"Đã tải: {countAdd}/{newInvoices.Count} - {completed:F2}% completed");
            }

            //Handle unsuccessful deserializion
            if (invDetail is { Success: true, Message: not null, Data: not null } && invDetail.Message.Contains("99"))
            {
                unDeserializedInvoices.Add((string)invDetail.Data);
                var completed = decimal.Divide(countAdd, newInvoices.Count) * 100;
                await notificationService
                    .SendAsync(UserId,
                               HubName.InvoiceMessage,
                               $"Đã tải: {countAdd}/{newInvoices.Count} - {completed:F2}% completed");
            }

            logger.LogInformation("Undeserializable count: {Count}", unDeserializedInvoices.Count);
            countAdd++;
        }

        return await WriteInvoices(deSerializedInvoices, unDeserializedInvoices, newInvoices.Count);
    }

    #endregion

    public async Task<byte[]?> ExportExcel(string taxCode, string from, string to)
    {
        var purchaseFilter = InvoiceFilterBuilder.StartBuilder()
                                                 .FromDate(from)
                                                 .ToDate(to)
                                                 .WithBuyer(taxCode)
                                                 .Build<InvoiceDetailDoc>();
        var purchaseResult = await mongoPurchaseInvoice.FindInvoices(filter: purchaseFilter,
                                                                     page: 1, size: int.MaxValue);
        var purchaseList = purchaseResult.Data.Select(inv => inv.ToDisplayModel())
                                         .ToList();
        logger.LogInformation("Number of purchase found: {}", purchaseList.Count);
        var soldFilter = InvoiceFilterBuilder.StartBuilder()
                                             .FromDate(from)
                                             .ToDate(to)
                                             .WithSeller(taxCode)
                                             .Build<SoldInvoiceDetail>();
        var soldResult =
            await soldInvoiceDetailRepository.FindInvoiceAsync(filter: soldFilter, page: 1, size: int.MaxValue);
        var soldList = soldResult.Data.Select(inv => inv.ToDisplayModel())
                                 .ToList();
        logger.LogInformation("Number of sold found: {}", soldList.Count);

        await notificationService.SendAsync(UserId,
                                            HubName.InvoiceMessage,
                                            $"Đang kết xuất dữ liệu của {purchaseList.Count} hóa đơn đầu vào và {soldList.Count} hóa đơn đầu ra.");

        var file = GenerateExcelFile(purchaseList, soldList, from, to);
        //await notificationService.SendAsync(UserId, HubName.InvoiceCount, "Finished.");
        return file;
    }

    public async Task<AppResponse> FindOne(string taxCode, string id)
    {
        var found = await mongoPurchaseInvoice.FindOneAsync(x => x.Id == id && x.Nmmst == taxCode);
        return found != null
            ? AppResponse.SuccessResponse(found.ToDisplayModel())
            : AppResponse.Error404("No invoice was found.");
    }

    #region Private method

    private async Task<AppResponse> WriteInvoices(List<InvoiceDetailModel> invoicesToSave,
                                                  List<string> unDeserializedInvoices, int total)
    {
        var totalSync = invoicesToSave.Count + unDeserializedInvoices.Count;
        try
        {
            var jsonOption = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            await notificationService.SendAsync(UserId, HubName.InvoiceMessage, "Writing to database...");
            var isInserted = await mongoPurchaseInvoice.InsertInvoicesAsync(invoicesToSave
                                                                            .Select(i => i.ToPurchaseInvoiceDetailBson(
                                                                                    jsonOption))
                                                                            .ToList());
            var isInsered2 = true;
            if (unDeserializedInvoices.Count > 0)
            {
                logger.LogWarning("{} undeserializable invoices, trying to save...", unDeserializedInvoices.Count);
                isInsered2 = await mongoPurchaseInvoice.InsertInvoicesAsync(unDeserializedInvoices
                                                                            .Select(i => i.ToPurchaseInvoiceDetailBson(
                                                                                    jsonOption))
                                                                            .ToList());
            }

            switch (isInserted)
            {
                case false when !isInsered2:
                    logger.LogWarning("Unable to save invoices. Operation terminated at {time}",
                                      DateTime.Now.ToLocalTime());
                    return AppResponse.Error("Nothing to insert.");
                case true:
                    logger.LogInformation("Finished syncing invoices at {time}",
                                          DateTime.Now.ToLocalTime());
                    break;
            }

            if (isInsered2)
            {
                logger.LogInformation("Finished syncing undeserializable invoices at {time}",
                                      DateTime.Now.ToLocalTime());
            }

            return new AppResponse
            {
                Success = true,
                Code = totalSync == total ? "200" : "207",
                Message = totalSync == total
                    ? $"{totalSync}/{total} hóa đơn đã được lưu."
                    : $"{totalSync}/{total} hóa đơn đã được lưu.\n" +
                      $"Hãy thử lại sau để tải về các hóa đơn chưa lưu thành công.",
                Data = new
                {
                    Total = total,
                    Success = totalSync,
                    Remaining = total - totalSync
                }
            };
        }
        catch (Exception e)
        {
            logger.LogError("Failed with Error: {mess}", e.Message);
            return AppResponse.Error500("Warning: saving invoices to database unsuccessfully due to an error occured.");
        }
    }


    private static byte[]? GenerateExcelFile(List<InvoiceDisplayDto> purchaseList,
                                             List<InvoiceDisplayDto> soldList,
                                             string from, string to)
    {
        if (purchaseList.Count == 0 && soldList.Count == 0)
        {
            return null;
        }

        var workbook = new Workbook
        {
            Version = ExcelVersion.Version2016
        };

        workbook.CreateEmptySheets(4);

        var orgName = string.Empty;
        var orgTaxId = string.Empty;

        if (purchaseList.Count > 0)
        {
            orgName += purchaseList[0].BuyerName.ToUpper();
            orgTaxId += purchaseList[0].BuyerTaxCode.ToUpper();
        }

        //TODO: add detail sheet for sold invoice
        var shPurchaseSummary = workbook.Worksheets[0];
        shPurchaseSummary.Name = "DauVao_Tong_hop";

        var shPurchaseDetail = workbook.Worksheets[1];
        shPurchaseDetail.Name = "DauVao_Chi_tiet";

        var shSoldSummary = workbook.Worksheets[2];
        shSoldSummary.Name = "DauRa_Tong_hop";
        var shSoldDetail = workbook.Worksheets[3];
        shSoldDetail.Name = "DauRa_Chi_tiet";
        var sheetCount = workbook.Worksheets.Count;

        shPurchaseDetail.Range[1, 1].Value = $"{orgName} - {orgTaxId}";
        shPurchaseDetail.Range[2, 1].Value = $"Chi tiết hóa đơn đầu vào - Từ {from} đến {to}";
        shPurchaseDetail.Range[1, 1, 2, 1].Style.Font.IsBold = true;

        shPurchaseSummary.Range[1, 1].Value = $"{purchaseList[0].BuyerName.ToUpper()} - {purchaseList[0].BuyerTaxCode}";
        shPurchaseSummary.Range[2, 1].Value = $"Danh sách hóa đơn đầu vào - Từ {from} đến {to}";
        shPurchaseSummary.Range[1, 1, 2, 1].Style.Font.IsBold = true;

        shSoldSummary.Range[1, 1].Value = $"{orgName} - {orgTaxId}";
        shSoldSummary.Range[2, 1].Value = $"Danh sách hóa đơn đầu ra - Từ {from} đến {to}";
        shSoldSummary.Range[1, 1, 2, 1].Style.Font.IsBold = true;

        shPurchaseDetail.Range[1, 1].Value = $"{orgName} - {orgTaxId}";
        shPurchaseDetail.Range[2, 1].Value = $"Chi tiết hóa đơn đầu ra - Từ {from} đến {to}";
        shPurchaseDetail.Range[1, 1, 2, 1].Style.Font.IsBold = true;

        const int titleRow = 4;

        List<string> soldSummaryTitles =
        [
            "Số hóa đơn", //1
            "Ký hiệu", //2
            "MST người mua", //3 
            "Tên người mua", //4
            "Ngày lập", //5
            "Ngày ký", //6
            "Ngày cấp mã", //7
            "Giá mua trước thuế", //8
            "Thuế GTGT", //9
            "Thành tiền", //10
            "Trạng thái", //11
        ];

        List<string> soldDetailTitles =
        [
            "Số hóa đơn", //1
            "Ký hiệu", //2
            "Mã số thuế", //3
            "Tên người mua", //4
            "Hàng hóa/dịch vụ", //5
            "Đơn vị tính", //6
            "Đơn giá", //7
            "Số lượng", //8
            "Giá mua trước thuế", //9
            "Thuế suất", //10
            "Chiết khấu", //11
            "Thuế GTGT", //12
            "Ngày lập", //13
            "Ngày ký", //14
            "Ngày cấp mã", //15
            "Trạng thái", //16
            "Loại hóa đơn", //17
            "Loại thuế suất" //18
        ];

        List<string> purchaseDetailTitles =
        [
            "Số hóa đơn", //1
            "Ký hiệu", //2
            "Mã số thuế", //3
            "Tên người bán", //4
            "Hàng hóa/dịch vụ", //5
            "Đơn vị tính", //6
            "Đơn giá", //7
            "Số lượng", //8
            "Giá mua trước thuế", //9
            "Thuế suất", //10
            "Chiết khấu", //11
            "Thuế GTGT", //12
            "Ngày lập", //13
            "Ngày ký", //14
            "Ngày cấp mã", //15
            "Trạng thái", //16
            "Loại hóa đơn", //17
            "Loại thuế suất" //18
        ];

        List<string> purchaseSummaryTitles =
        [
            "Số hóa đơn", //1
            "Ký hiệu", //2
            "MST người bán", //3
            "Tên người bán", //4
            "Ngày lập", //5
            "Ngày ký", //6
            "Ngày cấp mã", //7
            "Giá mua trước thuế", //8
            "Thuế GTGT", //9
            "Chiết khấu TM", //10
            "Phí", //11
            "Thành tiền", //12
            "Trạng thái", //13
            "Loại hóa đơn", //14
            "Cảnh báo nhà cung cấp" //15
        ];

        for (var i = 0; i < purchaseDetailTitles.Count; i++)
        {
            shPurchaseDetail.Range[titleRow, i + 1].Value = purchaseDetailTitles[i];
            shPurchaseDetail.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        for (var i = 0; i < purchaseSummaryTitles.Count; i++)
        {
            shPurchaseSummary.Range[titleRow, i + 1].Value = purchaseSummaryTitles[i];
            shPurchaseSummary.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        for (var i = 0; i < soldSummaryTitles.Count; i++)
        {
            shSoldSummary.Range[titleRow, i + 1].Value = soldSummaryTitles[i];
            shSoldSummary.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        for (var i = 0; i < soldDetailTitles.Count; i++)
        {
            shSoldDetail.Range[titleRow, i + 1].Value = soldDetailTitles[i];
            shSoldDetail.Range[titleRow, i + 1].BorderAround(LineStyleType.Thin);
        }

        shPurchaseDetail.Range[titleRow, 1, titleRow, purchaseDetailTitles.Count].Style.Font.IsBold = true;
        shPurchaseSummary.Range[titleRow, 1, titleRow, purchaseSummaryTitles.Count].Style.Font.IsBold = true;
        shSoldSummary.Range[titleRow, 1, titleRow, purchaseSummaryTitles.Count].Style.Font.IsBold = true;
        shSoldDetail.Range[titleRow, 1, titleRow, soldDetailTitles.Count].Style.Font.IsBold = true;
        var detailRow = 5;
        var purchaseSummaryRow = 5;

        #region PURCHASE INVOICE PROCESSING

        foreach (var inv in purchaseList.Where(inv => inv.GoodsDetail.Count != 0))
        {
            foreach (var item in inv.GoodsDetail)
            {
                #region Detail

                var unitPrice = item.UnitPrice;
                var preTaxPrice = item.PreTaxPrice;
                var vat = item.Tax;
                if (item.Name is not null
                    && (item.Name.Contains("chiết khấu", StringComparison.CurrentCultureIgnoreCase)
                        || item.Name.Contains("giảm giá", StringComparison.CurrentCultureIgnoreCase)))
                {
                    unitPrice = -unitPrice;
                    preTaxPrice = -preTaxPrice;
                    vat = -vat;
                }

                shPurchaseDetail.Range[detailRow, 1].Value2 = inv.InvoiceNumber;
                shPurchaseDetail.Range[detailRow, 2].Value2 = inv.InvoiceNotation;
                shPurchaseDetail.Range[detailRow, 3].Text = inv.SellerTaxCode;
                shPurchaseDetail.Range[detailRow, 4].Value2 = inv.SellerName;
                shPurchaseDetail.Range[detailRow, 5].Value2 = item.Name;
                shPurchaseDetail.Range[detailRow, 6].Value2 = item.UnitCount;

                shPurchaseDetail.Range[detailRow, 7].Value2 = unitPrice;
                shPurchaseDetail.Range[detailRow, 7].NumberFormat = "#,##0";

                shPurchaseDetail.Range[detailRow, 8].Value2 = item.Quantity;
                shPurchaseDetail.Range[detailRow, 8].NumberFormat = "#,##0";

                shPurchaseDetail.Range[detailRow, 9].Value2 = preTaxPrice;
                shPurchaseDetail.Range[detailRow, 9].NumberFormat = "#,##0";

                shPurchaseDetail.Range[detailRow, 10].Value2 = item.Rate;
                shPurchaseDetail.Range[detailRow, 10].NumberFormat = "0.0%";

                shPurchaseDetail.Range[detailRow, 11].Value2 = item.Discount;

                shPurchaseDetail.Range[detailRow, 12].Value2 = vat;
                shPurchaseDetail.Range[detailRow, 12].NumberFormat = "#,##0";
                shPurchaseDetail.Range[detailRow, 13].Value2 = inv.CreationDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 13].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 14].Value2 = inv.SigningDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 14].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 15].Value2 = inv.IssueDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 14].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 16].Value2 = inv.Status;
                shPurchaseDetail.Range[detailRow, 17].Value2 = inv.InvoiceType;
                shPurchaseDetail.Range[detailRow, 18].Value2 = item.TaxType;

                #endregion

                detailRow++;
            }

            #region Summary

            shPurchaseSummary.Range[purchaseSummaryRow, 1].Value2 = inv.InvoiceNumber;
            shPurchaseSummary.Range[purchaseSummaryRow, 2].Value2 = inv.InvoiceNotation;
            shPurchaseSummary.Range[purchaseSummaryRow, 3].Text = inv.SellerTaxCode;
            shPurchaseSummary.Range[purchaseSummaryRow, 4].Value2 = inv.SellerName;
            shPurchaseSummary.Range[purchaseSummaryRow, 5].Value2 = inv.CreationDate?.ToLocalTime();
            shPurchaseSummary.Range[purchaseSummaryRow, 5].Style.NumberFormat = "dd/mm/yyyy";
            shPurchaseSummary.Range[purchaseSummaryRow, 6].Value2 = inv.SigningDate?.ToLocalTime();
            shPurchaseSummary.Range[purchaseSummaryRow, 6].Style.NumberFormat = "dd/mm/yyyy";
            shPurchaseSummary.Range[purchaseSummaryRow, 7].Value2 = inv.IssueDate?.ToLocalTime();
            shPurchaseSummary.Range[purchaseSummaryRow, 7].Style.NumberFormat = "dd/mm/yyyy";
            shPurchaseSummary.Range[purchaseSummaryRow, 8].Value2 = inv.TotalPrice;
            shPurchaseSummary.Range[purchaseSummaryRow, 8].NumberFormat = "#,##0";

            shPurchaseSummary.Range[purchaseSummaryRow, 9].Value2 = inv.Vat;
            shPurchaseSummary.Range[purchaseSummaryRow, 9].NumberFormat = "#,##0";

            shPurchaseSummary.Range[purchaseSummaryRow, 10].Value2 = inv.ChietKhau;
            shPurchaseSummary.Range[purchaseSummaryRow, 10].NumberFormat = "#,##0";

            shPurchaseSummary.Range[purchaseSummaryRow, 11].Value2 = inv.Phi;
            shPurchaseSummary.Range[purchaseSummaryRow, 11].NumberFormat = "#,##0";

            shPurchaseSummary.Range[purchaseSummaryRow, 12].Value2 = inv.TotalPriceVat;
            shPurchaseSummary.Range[purchaseSummaryRow, 12].NumberFormat = "#,##0";

            shPurchaseSummary.Range[purchaseSummaryRow, 13].Value2 = inv.Status;
            shPurchaseSummary.Range[purchaseSummaryRow, 14].Value2 = inv.InvoiceType;
            shPurchaseSummary.Range[purchaseSummaryRow, 15].Value2 = inv.Risk is null or false ? "OK" : "Rủi ro";

            #endregion

            purchaseSummaryRow++;
        }

        #endregion


        #region SOLD INVOICE PROCESSING

        if (soldList.Count > 0)
        {
            var soldSummaryRow = 5;
            var soldDetailRow = 5;
            foreach (var inv in soldList)
            {
                shSoldSummary.Range[soldSummaryRow, 1].Value2 = inv.InvoiceNumber;
                shSoldSummary.Range[soldSummaryRow, 2].Value2 = inv.InvoiceNotation;
                shSoldSummary.Range[soldSummaryRow, 3].Value2 = inv.BuyerTaxCode;
                shSoldSummary.Range[soldSummaryRow, 4].Value2 = inv.BuyerName;
                shSoldSummary.Range[soldSummaryRow, 5].Value2 = inv.CreationDate?.ToLocalTime();
                shSoldSummary.Range[soldSummaryRow, 5].Style.NumberFormat = "dd/mm/yyyy";
                shSoldSummary.Range[soldSummaryRow, 6].Value2 = inv.SigningDate?.ToLocalTime();
                shSoldSummary.Range[soldSummaryRow, 6].Style.NumberFormat = "dd/mm/yyyy";
                shSoldSummary.Range[soldSummaryRow, 7].Value2 = inv.IssueDate?.ToLocalTime();
                shSoldSummary.Range[soldSummaryRow, 7].Style.NumberFormat = "dd/mm/yyyy";
                shSoldSummary.Range[soldSummaryRow, 8].Value2 = inv.TotalPrice;
                shSoldSummary.Range[soldSummaryRow, 8].NumberFormat = "#,##0";
                shSoldSummary.Range[soldSummaryRow, 9].Value2 = inv.Vat;
                shSoldSummary.Range[soldSummaryRow, 9].NumberFormat = "#,##0";
                shSoldSummary.Range[soldSummaryRow, 10].Value2 = inv.TotalPriceVat;
                shSoldSummary.Range[soldSummaryRow, 10].NumberFormat = "#,##0";
                shSoldSummary.Range[soldSummaryRow, 11].Value2 = inv.Status;

                soldSummaryRow++;
            }

            foreach (var inv in soldList.Where(x => x.GoodsDetail.Count != 0))
            {
                foreach (var item in inv.GoodsDetail)
                {
                    var unitPrice = item.UnitPrice;
                    var preTaxPrice = item.PreTaxPrice;
                    var vat = item.Tax;
                    if (item.Name is not null
                        && (item.Name.Contains("chiết khấu", StringComparison.CurrentCultureIgnoreCase)
                            || item.Name.Contains("giảm giá", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        unitPrice = -unitPrice;
                        preTaxPrice = -preTaxPrice;
                        vat = -vat;
                    }

                    shSoldDetail.Range[soldDetailRow, 1].Value2 = inv.InvoiceNumber;
                    shSoldDetail.Range[soldDetailRow, 2].Value2 = inv.InvoiceNotation;
                    shSoldDetail.Range[soldDetailRow, 3].Text = inv.BuyerTaxCode;
                    shSoldDetail.Range[soldDetailRow, 4].Value2 = inv.BuyerName;
                    shSoldDetail.Range[soldDetailRow, 5].Value2 = item.Name;
                    shSoldDetail.Range[soldDetailRow, 6].Value2 = item.UnitCount;

                    shSoldDetail.Range[soldDetailRow, 7].Value2 = unitPrice;
                    shSoldDetail.Range[soldDetailRow, 7].NumberFormat = "#,##0";

                    shSoldDetail.Range[soldDetailRow, 8].Value2 = item.Quantity;
                    shSoldDetail.Range[soldDetailRow, 8].NumberFormat = "#,##0";

                    shSoldDetail.Range[soldDetailRow, 9].Value2 = preTaxPrice;
                    shSoldDetail.Range[soldDetailRow, 9].NumberFormat = "#,##0";

                    shSoldDetail.Range[soldDetailRow, 10].Value2 = item.Rate;
                    shSoldDetail.Range[soldDetailRow, 10].NumberFormat = "0.0%";

                    shSoldDetail.Range[soldDetailRow, 11].Value2 = item.Discount;

                    shSoldDetail.Range[soldDetailRow, 12].Value2 = vat;
                    shSoldDetail.Range[soldDetailRow, 12].NumberFormat = "#,##0";
                    shSoldDetail.Range[soldDetailRow, 13].Value2 = inv.CreationDate?.ToLocalTime();
                    shSoldDetail.Range[soldDetailRow, 13].Style.NumberFormat = "dd/mm/yyyy";

                    shSoldDetail.Range[soldDetailRow, 14].Value2 = inv.SigningDate?.ToLocalTime();
                    shSoldDetail.Range[soldDetailRow, 14].Style.NumberFormat = "dd/mm/yyyy";

                    shSoldDetail.Range[soldDetailRow, 15].Value2 = inv.IssueDate?.ToLocalTime();
                    shSoldDetail.Range[soldDetailRow, 14].Style.NumberFormat = "dd/mm/yyyy";

                    shSoldDetail.Range[soldDetailRow, 16].Value2 = inv.Status;
                    shSoldDetail.Range[soldDetailRow, 17].Value2 = inv.InvoiceType;
                    shSoldDetail.Range[soldDetailRow, 18].Value2 = item.TaxType;

                    soldDetailRow++;
                }
            }

            shSoldSummary.Range[4, 1, soldSummaryRow - 1, 3].AutoFitColumns();
            shSoldSummary.Range[4, 5, soldSummaryRow - 1, 11].AutoFitColumns();
            shSoldSummary.AutoFilters.Range = shSoldSummary.Range[$"A{titleRow}:X{soldSummaryRow - 1}"];
            shSoldSummary.Range[3, 1].FormulaR1C1 =
                $"\"Tổng số hóa đơn: \"&COUNT(A{titleRow + 1}:A{soldSummaryRow - 1})";
            foreach (var cell in shSoldSummary.Range[4, 1, soldSummaryRow - 1, 11])
            {
                cell.BorderAround(LineStyleType.Thin);
            }

            shSoldDetail.Range[4, 1, soldDetailRow - 1, 3].AutoFitColumns();
            shSoldDetail.Range[4, 6, soldDetailRow - 1, 16].AutoFitColumns();
            shSoldDetail.AutoFilters.Range = shSoldDetail.Range[$"A{titleRow}:X{soldDetailRow - 1}"];
            shSoldDetail.Range[3, 1].FormulaR1C1 = $"\"Tổng số hóa đơn: \"&COUNT(A{titleRow + 1}:A{soldDetailRow - 1})";
            foreach (var cell in shSoldDetail.Range[4, 1, soldDetailRow - 1, 18])
            {
                cell.BorderAround(LineStyleType.Thin);
            }
        }

        #endregion

        shPurchaseDetail.Range[4, 1, detailRow - 1, 3].AutoFitColumns();
        shPurchaseDetail.Range[4, 6, detailRow - 1, 16].AutoFitColumns();

        shPurchaseSummary.Range[4, 1, purchaseSummaryRow - 1, 3].AutoFitColumns();
        shPurchaseSummary.Range[4, 5, purchaseSummaryRow - 1, 13].AutoFitColumns();


        #region Formula and filter

        shPurchaseSummary.AutoFilters.Range = shPurchaseSummary.Range[$"A{titleRow}:X{detailRow - 1}"];
        shPurchaseDetail.AutoFilters.Range = shPurchaseDetail.Range[$"A{titleRow}:X{purchaseSummaryRow - 1}"];

        shPurchaseSummary.Range[3, 1].FormulaR1C1 =
            $"\"Tổng số hóa đơn: \"&COUNT(A{titleRow + 1}:A{purchaseSummaryRow - 1})";
        shPurchaseDetail.Range[3, 1].FormulaR1C1 =
            $"\"Tổng số hóa đơn: \"&COUNT(UNIQUE(A{titleRow + 1}:A{detailRow - 1}))";

        for (var i = 8; i <= 12; i++)
        {
            shPurchaseSummary.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{purchaseSummaryRow - 1}C{i})";
            shPurchaseSummary.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shPurchaseSummary.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 7; i <= 11; i++)
        {
            shPurchaseDetail.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{detailRow - 1}C{i})";
            shPurchaseDetail.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shPurchaseDetail.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 8; i <= 12; i++)
        {
            shSoldSummary.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{detailRow - 1}C{i})";
            shSoldSummary.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shSoldSummary.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 7; i <= 11; i++)
        {
            shSoldDetail.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{detailRow - 1}C{i})";
            shSoldDetail.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shSoldDetail.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        #endregion

        foreach (var cell in shPurchaseDetail.Range[4, 1, detailRow - 1, 18])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in shSoldDetail.Range[4, 1, detailRow - 1, 18])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in shPurchaseSummary.Range[4, 1, purchaseSummaryRow - 1, 15])
        {
            cell.BorderAround(LineStyleType.Thin);
        }


        using var stream = new MemoryStream();
        workbook.SaveToStream(stream, FileFormat.Version2016);
        return stream.ToArray();
    }

    #endregion

    private async Task<bool> IsPurchaseInvoiceDuplicate(InvoiceModel invoice)
    {
        var filter = InvoiceFilterBuilder.StartBuilder()
                                         .WitdId(invoice.Id)
                                         .Build<InvoiceDetailDoc>();

        return await mongoPurchaseInvoice.InvoiceExist(filter);
    }
}