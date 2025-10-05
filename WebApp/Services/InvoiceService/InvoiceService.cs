using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Spire.Xls;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Mongo.FilterBuilder;
using WebApp.Mongo.Mapper;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.NotificationService;
using WebApp.Services.RestService;
using WebApp.Services.RestService.Dto;
using WebApp.Services.RiskCompanyService;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services.InvoiceService;

public interface IInvoiceService
{
    Task<ResponseBase> DeletePurchaseInvoicesAsync(List<string> ids);
    Task<ResponseBase> DeleteSoldInvoicesAsync(List<string> ids);

    // Define methods for the InvoiceService here
    /// <summary>
    /// Export current invoices list to an Excel workbook
    /// </summary>
    /// <param name="taxCode">Company's taxId</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns></returns>
    Task<byte[]?> ExportExcel(string taxCode, string from, string to);
    /// <summary>
    /// Extract invoice's detail information from the hoadondientu API
    /// </summary>
    /// <param name="token">The JWT to access hoadondientu API</param>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns></returns>
    Task<ResponseBase> ExtractPurchaseInvoices(string token, string from, string to, int[]? invoiceTypes);
    Task<ResponseBase> GetSoldInvoiceFromService(string token, string from, string to);

    /// <summary>
    /// Query purchase invoices of a given company's taxId
    /// </summary>
    /// <param name="taxCode">The company's taxId</param>
    /// <param name="invoiceParams">Request parameters to build the filter for query</param>
    /// <returns></returns>
    Task<ResponseBase> QueryPurchaseInvoices(string taxCode, InvoiceRequestParam invoiceParams);
    /// <summary>
    /// Upload purchase invoice from xml files
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    Task<ResponseBase> UploadPurchaseInvoices(List<IFormFile> files);
}

//TODO: refactor this service class to replace the old InvoiceAppService
public partial class InvoiceService(IUserManager userManager,
                                    ILogger<InvoiceService> logger,
                                    IInvoiceMongoRepository mongoPurchaseInvoice,
                                    ISoldInvoiceMongoRepository mongoSoldInvoice,
                                    IRestAppService restService,
                                    IRiskCompanyAppService riskService,
                                    IInvoiceHistoryAppService invoiceHistoryAppService,
                                    ISoldInvoiceDetailRepository soldInvoiceDetailRepository,
                                    IErrorInvoiceRepository errorInvoiceRepository,
                                    INotificationAppService notificationService)
     : BaseAppService(userManager), IInvoiceService
{
    private JwtSecurityTokenHandler TokenHandler => new JwtSecurityTokenHandler();

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


    private async Task<(bool Success, int ErrorCount, List<PurchaseInvoiceErrorDisplay> Error)> WriteDeserializableInvoices(List<InvoiceDetailModel> invoices)
    {
        List<PurchaseInvoiceErrorDisplay> errorList = [];
        if (invoices.Count == 0)
        {
            Console.WriteLine("No invoice to convert");
            return (true, 0, errorList);
        }
        Console.WriteLine($"Found {invoices.Count} invoices.");
        try
        {
            var jsonOption = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var errorCount = 0;
            List<InvoiceDetailDoc> listToInsert = [];
            foreach (var invoice in invoices)
            {
                try
                {
                    var invoiceDoc = invoice.ToPurchaseInvoiceDetailBson(jsonOption);
                    listToInsert.Add(invoiceDoc);
                }
                catch (Exception e)
                {
                    logger.LogError("Failed to convert invoice: {shdon} - {mst} - {date}",
                                    invoice.Shdon, invoice.Nbmst, invoice.Tdlap);
                    logger.LogInformation("Trying to serialize the invoice to string and save for further inspection.");
                    await errorInvoiceRepository.InsertAsync(new ErrorInvoiceDoc
                    {
                        OrgId = WorkingOrg.ToGuid().ToString(),
                        InvoiceNumber = invoice.Shdon,
                        BuyerTaxId = invoice.Nmmst,
                        SellerTaxId = invoice.Nbmst,
                        Content = JsonSerializer.Serialize(invoice, jsonOption),
                        InvoiceDate = invoice.Tdlap,
                        Type = 0,
                        Message = e.Message
                    });
                    errorList.Add(new PurchaseInvoiceErrorDisplay
                    {
                        InvoiceNumber = invoice.Shdon.ToString(),
                        SellerTaxId = invoice.Nbmst,
                        SellerName = invoice.Nmmst,
                        IssueDate = invoice.Tdlap,
                        ErrorMessage = e.Message
                    });
                    errorCount++;
                }
            }

            var result = await mongoPurchaseInvoice.InsertInvoicesAsync(listToInsert);
            return (result, errorCount, errorList);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return (false, 0, errorList);
        }
    }

    private async Task<(bool Success, int ErrorCount, List<PurchaseInvoiceErrorDisplay> Error)> WriteUndeserializableInvoices(List<string> invoices)
    {
        List<PurchaseInvoiceErrorDisplay> errorList = [];
        if (invoices.Count == 0)
        {
            Console.WriteLine("No invoice to convert");
            return (true, 0, errorList);
        }
        Console.WriteLine($"Found {invoices.Count} invoices.");
        try
        {
            List<InvoiceDetailDoc> listToInsert = [];
            var errorCount = 0;
            foreach (string invoice in invoices)
            {
                try
                {
                    var invoiceDoc = invoice.ToPurchaseInvoiceDetailBson();
                    Console.WriteLine("Converted: " + invoiceDoc.Shdon);
                    listToInsert.Add(invoiceDoc);
                }
                catch (Exception e)
                {
                    var invNumber = invoice.ExtractValueRegex(InvoiceNumberRegex())?.ToInt();
                    var sellerTaxId = invoice.ExtractValueRegex(SellerTaxIdRegex());
                    logger.LogError("Failed to convert invoice {number} - {seller}", invNumber, sellerTaxId);
                    logger.LogWarning(e.Message);
                    logger.LogWarning(e.StackTrace);
                    await errorInvoiceRepository.InsertAsync(new ErrorInvoiceDoc
                    {
                        OrgId = WorkingOrg.ToGuid().ToString(),
                        Content = invoice,
                        Message = e.Message,
                        InvoiceDate = invoice.ExtractValueRegex(InvoiceDateRegex())?.ToDateTime(),
                        InvoiceNumber = invNumber,
                        BuyerTaxId = invoice.ExtractValueRegex(BuyerTaxIdRegex()),
                        SellerTaxId = sellerTaxId,
                        Type = 0,
                    });
                    errorCount++;
                }
            }

            var result = await mongoPurchaseInvoice.InsertInvoicesAsync(listToInsert);
            return (result, errorCount, errorList);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return (false, 0, errorList);
        }
    }


    //TODO: separate this method into two methods: one for deserializable invoices and one for un-deserializable invoices
    private async Task<ResponseBase> WriteInvoices(List<InvoiceDetailModel> deserializedInvoices,
                                                  List<string> unDeserializedInvoices, int total)
    {
        var totalSync = deserializedInvoices.Count + unDeserializedInvoices.Count;
        try
        {
            var jsonOption = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var listToInsert = new List<InvoiceDetailDoc>();
            var errorList = new List<string?>();
            foreach (var invoice in deserializedInvoices)
            {
                try
                {
                    var doc = invoice.ToPurchaseInvoiceDetailBson(jsonOption);
                    listToInsert.Add(doc);
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to deserialize invoice to BSON: {error}", ex.Message);
                    await notificationService.SendAsync(UserId,
                                                        HubName.InvoiceMessage,
                                                        "Failed to process some invoices due to conversion error.");
                    errorList.Add(invoice.Shdon?.ToString());
                }
            }

            if (unDeserializedInvoices.Count > 0)
            {
                // Regex to match "shdon": "value" or "shdon": value (string or non-string)
                var regex = new Regex(@"""shdon""\s*:\s*(?:""(?<value>[^""]*)""|(?<value>[^,\}\s]+))");
                foreach (string unDeserializedInvoice in unDeserializedInvoices)
                {
                    try
                    {
                        var doc = unDeserializedInvoice.ToPurchaseInvoiceDetailBson();
                        listToInsert.Add(doc);
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        logger.LogError("Failed to convert undeserialized invoice to BSON: {error}", ex.Message);
                        // Optionally notify the user about the failed conversion
                        await notificationService.SendAsync(UserId,
                                                            HubName.InvoiceMessage,
                                                            "Failed to process some undeserialized invoices due to conversion error.");
                        var match = regex.Match(unDeserializedInvoice);
                        if (match.Success)
                        {
                            string value = match.Groups["value"].Value;
                            errorList.Add(value);
                        }
                    }
                }
            }

            //Build a messages for error
            var message = listToInsert.Count == 0
                ? "Không có hóa đơn mới. "
                : $"Tìm thấy {listToInsert.Count} hóa đơn mới. ";
            var errorMessage =
                errorList.Count == 0 ? "" : $" {errorList.Count} hóa đơn không thể lưu do lỗi định dạng.";
            //Notify user about success/failure
            await notificationService.SendAsync(UserId, HubName.InvoiceMessage, message + errorMessage);

            var isInserted = await mongoPurchaseInvoice.InsertInvoicesAsync(listToInsert); //Insert into DB
            return new ResponseBase
            {
                Success = isInserted,
                Code = totalSync == total ? "200" : "207",
                TotalCount = totalSync,
                Message = totalSync == total
                    ? $"{totalSync}/{total} hóa đơn đã được lưu."
                    : $"{totalSync}/{total} hóa đơn đã được lưu.\n" +
                      $"Hãy thử lại sau để tải về các hóa đơn chưa lưu thành công.",
                Data = new
                {
                    Total = total,
                    Success = totalSync,
                    Remaining = total - totalSync,
                    Error = errorList
                }
            };
        }
        catch (Exception e)
        {
            logger.LogError("Failed with Error: {mess}", e.Message);
            return ResponseBase.Error500("Warning: saving invoices to database unsuccessfully due to an error occured.");
        }
    }

    private static async Task<InvoiceDetailDoc> ReadInvoiceFromXml(IFormFile file)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;
        var xmlDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        var invoice = xmlDocument.ToInvoiceModel();
        return invoice;
    }

    private byte[]? GenerateExcelFile(List<InvoiceDisplayDto> purchaseList,
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

        shSoldDetail.Range[1, 1].Value = $"{orgName} - {orgTaxId}";
        shSoldDetail.Range[2, 1].Value = $"Chi tiết hóa đơn đầu ra - Từ {from} đến {to}";
        shSoldDetail.Range[1, 1, 2, 1].Style.Font.IsBold = true;

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
            "Thuế GTGT", //9,
            "Phí khác", //10
            "Thành tiền", //11,
            "Trạng thái", //12
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
            "Cảnh báo nhà cung cấp", //15,
            "Link tra cứu", //16
            "Mã tra cứu" //17
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
        var soldSummaryRow = 5;
        var soldDetailRow = 5;

        #region PURCHASE INVOICE PROCESSING

        foreach (var inv in purchaseList)
        {
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
            shPurchaseSummary.Range[purchaseSummaryRow, 16].Value2 = inv.LookUpUrl;
            shPurchaseSummary.Range[purchaseSummaryRow, 17].Value2 = inv.LookUpCode;

            #endregion

            if (inv.GoodsDetail.IsNullOrEmpty())
            {
                purchaseSummaryRow++;

                shPurchaseDetail.Range[detailRow, 1].Value2 = inv.InvoiceNumber;
                shPurchaseDetail.Range[detailRow, 2].Value2 = inv.InvoiceNotation;
                shPurchaseDetail.Range[detailRow, 3].Text = inv.SellerTaxCode;
                shPurchaseDetail.Range[detailRow, 4].Value2 = inv.SellerName;
                shPurchaseDetail.Range[detailRow, 5].Value2 = string.Empty;
                shPurchaseDetail.Range[detailRow, 6].Value2 = string.Empty;

                shPurchaseDetail.Range[detailRow, 7].Value2 = null;
                //shPurchaseDetail.Range[detailRow, 7].NumberFormat = "#,##0";

                shPurchaseDetail.Range[detailRow, 8].Value2 = null;
                //shPurchaseDetail.Range[detailRow, 8].NumberFormat = "#,##0";

                shPurchaseDetail.Range[detailRow, 9].Value2 = inv.TotalPrice;
                shPurchaseDetail.Range[detailRow, 9].NumberFormat = "#,##0";

                // shPurchaseDetail.Range[detailRow, 10].Value2 = item.Rate;
                // shPurchaseDetail.Range[detailRow, 10].NumberFormat = "0.0%";

                //shPurchaseDetail.Range[detailRow, 11].Value2 = item.Discount;

                shPurchaseDetail.Range[detailRow, 12].Value2 = inv.Vat;
                shPurchaseDetail.Range[detailRow, 12].NumberFormat = "#,##0";
                shPurchaseDetail.Range[detailRow, 13].Value2 = inv.CreationDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 13].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 14].Value2 = inv.SigningDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 14].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 15].Value2 = inv.IssueDate?.ToLocalTime();
                shPurchaseDetail.Range[detailRow, 14].Style.NumberFormat = "dd/mm/yyyy";

                shPurchaseDetail.Range[detailRow, 16].Value2 = inv.Status;
                shPurchaseDetail.Range[detailRow, 17].Value2 = inv.InvoiceType;
                //shPurchaseDetail.Range[detailRow, 18].Value2 = item.TaxType;
                detailRow++;
                continue;
            }

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

            purchaseSummaryRow++;
        }

        #endregion


        #region SOLD INVOICE PROCESSING

        if (soldList.Count > 0)
        {
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
                shSoldSummary.Range[soldSummaryRow, 10].Value2 = inv.TotalOtherFee ?? 0;
                shSoldSummary.Range[soldSummaryRow, 10].NumberFormat = "#,##0";
                shSoldSummary.Range[soldSummaryRow, 11].Value2 = inv.TotalPriceVat;
                shSoldSummary.Range[soldSummaryRow, 11].NumberFormat = "#,##0";
                
                shSoldSummary.Range[soldSummaryRow, 12].Value2 = inv.Status;

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
                    shSoldDetail.Range[soldDetailRow, 15].Style.NumberFormat = "dd/mm/yyyy";

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


        #region Formula, filter and formatting

        shPurchaseSummary.AutoFilters.Range = shPurchaseSummary.Range[$"A{titleRow}:X{detailRow - 1}"];
        shPurchaseDetail.AutoFilters.Range = shPurchaseDetail.Range[$"A{titleRow}:X{detailRow - 1}"];

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

        for (var i = 7; i <= 12; i++)
        {
            shPurchaseDetail.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{detailRow - 1}C{i})";
            shPurchaseDetail.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shPurchaseDetail.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 8; i <= 12; i++)
        {
            shSoldSummary.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{soldDetailRow - 1}C{i})";
            shSoldSummary.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shSoldSummary.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        for (var i = 7; i <= 12; i++)
        {
            shSoldDetail.Range[titleRow - 1, i].FormulaR1C1 = $"=SUBTOTAL(9,R5C{i}:R{soldDetailRow - 1}C{i})";
            shSoldDetail.Range[titleRow - 1, i].NumberFormat = "#,##0";
            shSoldDetail.Range[titleRow - 1, i].Style.Font.IsBold = true;
        }

        //Bordering the tables:
        foreach (var cell in shPurchaseDetail.Range[4, 1, detailRow - 1, purchaseDetailTitles.Count])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in shSoldDetail.Range[4, 1, soldDetailRow - 1, soldDetailTitles.Count])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in shPurchaseSummary.Range[4, 1, purchaseSummaryRow - 1, purchaseSummaryTitles.Count])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        foreach (var cell in shSoldSummary.Range[4, 1, soldSummaryRow - 1, soldSummaryTitles.Count])
        {
            cell.BorderAround(LineStyleType.Thin);
        }

        #endregion




        using var stream = new MemoryStream();
        workbook.SaveToStream(stream, FileFormat.Version2016);
        return stream.ToArray();
    }

    [GeneratedRegex("""
                    "shdon"\s*:\s*(?:"(?<value>[^"]*)"|(?<value>[^,\}\s]+))
                    """)]
    private static partial Regex InvoiceNumberRegex();

    [GeneratedRegex("""
                    "nmmst"\s*:\s*(?:"(?<value>[^"]*)"|(?<value>[^,\}\s]+))
                    """)]
    private static partial Regex BuyerTaxIdRegex();

    [GeneratedRegex("""
                    "nbmst"\s*:\s*(?:"(?<value>[^"]*)"|(?<value>[^,\}\s]+))
                    """)]
    private static partial Regex SellerTaxIdRegex();

    [GeneratedRegex("""
                    "nbten"\s*:\s*(?:"(?<value>[^"]*)"|(?<value>[^,\}\s]+))
                    """)]
    private static partial Regex SellerNameRegex();

    [GeneratedRegex("""
                    "tdlap"\s*:\s*(?:"(?<value>[^"]*)"|(?<value>[^,\}\s]+))
                    """)]
    private static partial Regex InvoiceDateRegex();
}