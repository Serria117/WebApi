using Newtonsoft.Json;
using Spire.Xls;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Mongo.FilterBuilder;
using WebApp.Payloads;
using WebApp.Payloads.Messages;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService.Dto.SoldInvoice;
using WebApp.Utils;

namespace WebApp.Services.InvoiceService;

public partial class InvoiceService
{
    public async Task<ResponseEntity> DeleteSoldInvoicesAsync(List<string> ids)
    {
        var result = await soldInvoiceDetailRepository.DeleteSoldInvoice(ids);
        return result ? ResponseEntity.Ok() : ResponseEntity.Error("Failed to delete invoices");
    }

    public async Task<ResponseEntity> GetSoldInvoiceFromService(string token, string from, string to)
    {
        var response = await restService.GetSoldInvoiceInRangeAsync(token, from, to);
        var countFromResponse = response.TotalCount ?? 0;
        if (countFromResponse == 0) return new ResponseEntity
        {
            TotalCount = countFromResponse,
            Message = $"Không có hóa đơn bán hàng phát sinh từ ngày {from} đến ngày {to}"
        };

        if (response is not { Success: true, Data: not null })
        {
            return new ResponseEntity
            {
                Success = false,
                Message = "Đã xảy ra lỗi trong quá trình tải hóa đơn. Hãy thử lại sau!"
            };
        }
        var duplicateCount = 0;
        var downloadCount = 0;
        var insertedCount = 0;
        var errorCount = 0;
        var errorMessages = new List<string>();
        var errorInvoiceList = new List<SoldInvoiceErrorDisplay>();

        var responseData = (List<SoldInvoiceModel>)response.Data;
        var invoiceList = new List<SoldInvoiceModel>(); //Store invoices to be inserted into mongodb

        foreach (var inv in responseData)
        {
            if (await IsDuplicate(inv))
            {
                duplicateCount++;
                continue;
            }
            invoiceList.Add(inv);
        }

        if (invoiceList.Count == 0)
        {
            return new ResponseEntity
            {
                Success = true,
                Message = "Không có hóa đơn mới cần tải về"
            };
        }

        var deserializedList = new List<SoldInvoiceDetail>();

        foreach (var inv in invoiceList)
        {
            var invoiceDetailResponse = await restService.GetSoldInvoiceDetail(token, inv);
            if (invoiceDetailResponse is { Success: true, Data: SoldInvoiceDetail })
            {
                try
                {
                    var invoiceDetailJson = JsonConvert.SerializeObject(invoiceDetailResponse.Data);
                    var invoiceDetail = JsonConvert.DeserializeObject<SoldInvoiceDetail>(invoiceDetailJson);

                    if (invoiceDetail is not null)
                    {
                        downloadCount++;
                        await notificationService.SendAsync(UserId,
                                                            HubName.InvoiceStatus,
                                                            InvoiceMessage.Create(
                                                                saved: downloadCount,
                                                                total: invoiceList.Count));
                        deserializedList.Add(invoiceDetail);
                    }
                    else
                    {
                        throw new JsonSerializationException();
                    }
                }
                catch (JsonReaderException ex)
                {
                    logger.LogWarning($"Incompartible JSON format. {inv.Shdon} - {inv.Nmmst}");
                    logger.LogWarning(ex.Message);
                    errorCount++;
                    errorInvoiceList.Add(new SoldInvoiceErrorDisplay
                    {
                        InvoiceNumber = inv.Shdon.ToString(),
                        BuyerTaxId = inv.Nmmst,
                        BuyerName = inv.Nmten,
                        ErrorMessage = "Dữ liệu hóa đơn lỗi khi đọc từ dịch vụ.",
                        IssueDate = inv.Tdlap

                    });
                    await errorInvoiceRepository.InsertAsync(new ErrorInvoiceDoc
                    {
                        BuyerTaxId = inv.Nmmst,
                        InvoiceNumber = inv.Shdon,
                        OrgId = WorkingOrg,
                        Content = invoiceDetailResponse.Data.ToString(),
                        InvoiceDate = DateTime.UtcNow.ToLocalTime(),
                        Message = ex.Message,
                        Period = $"{from} - {to}"
                    });
                }
                catch (JsonSerializationException ex)
                {
                    logger.LogWarning($"Failed to serialized invoice {inv.Shdon} - {inv.Nmmst}");
                    errorCount++;
                    errorInvoiceList.Add(new SoldInvoiceErrorDisplay
                    {
                        InvoiceNumber = inv.Shdon.ToString(),
                        BuyerTaxId = inv.Nmmst,
                        BuyerName = inv.Nmten,
                        ErrorMessage = "Lỗi trong quá trình ghi hóa đơn vào hệ thống.",
                        IssueDate = inv.Tdlap

                    });
                    await errorInvoiceRepository.InsertAsync(new ErrorInvoiceDoc
                    {
                        BuyerTaxId = inv.Nmmst,
                        InvoiceNumber = inv.Shdon,
                        OrgId = WorkingOrg,
                        Content = invoiceDetailResponse.Data.ToString(),
                        InvoiceDate = DateTime.UtcNow.ToLocalTime(),
                        Message = ex.Message,
                        Period = $"{from} - {to}"
                    });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex.Message);
                    errorCount++;
                    errorInvoiceList.Add(new SoldInvoiceErrorDisplay
                    {
                        InvoiceNumber = inv.Shdon.ToString(),
                        BuyerTaxId = inv.Nmmst,
                        BuyerName = inv.Nmten,
                        ErrorMessage = "Lỗi không xác định.",
                        IssueDate = inv.Tdlap

                    });
                    await errorInvoiceRepository.InsertAsync(new ErrorInvoiceDoc
                    {
                        BuyerTaxId = inv.Nmmst,
                        InvoiceNumber = inv.Shdon,
                        OrgId = WorkingOrg,
                        Content = invoiceDetailResponse.Data.ToString(),
                        InvoiceDate = DateTime.UtcNow.ToLocalTime(),
                        Message = ex.Message,
                        Period = $"{from} - {to}"
                    });
                }
                finally
                {
                    //Insert every batch of 10 invoice to save time
                    if (deserializedList.Count == 10)
                    {
                        insertedCount += await soldInvoiceDetailRepository.InsertManyInvoiceAsync(deserializedList);
                        deserializedList.Clear(); //Clear the list after inserted
                    }
                }
            }
            if (invoiceDetailResponse is { Success: true, Data: string })
            {
                try
                {
                    var deserializedResult = JsonConvert.DeserializeObject<SoldInvoiceDetail>((string)invoiceDetailResponse.Data);
                    if (deserializedResult is not null)
                    {
                        deserializedList.Add(deserializedResult);
                        downloadCount++;
                        await notificationService.SendAsync(UserId,
                                                            HubName.InvoiceStatus,
                                                            InvoiceMessage.Create(
                                                                saved: downloadCount,
                                                                total: invoiceList.Count));
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    errorInvoiceList.Add(new SoldInvoiceErrorDisplay
                    {
                        InvoiceNumber = inv.Shdon.ToString(),
                        BuyerTaxId = inv.Nmmst,
                        BuyerName = inv.Nmten,
                        ErrorMessage = "Lỗi không xác định.",
                        IssueDate = inv.Tdlap

                    });
                    logger.LogWarning(ex.Message);
                    await errorInvoiceRepository.InsertAsync(new ErrorInvoiceDoc
                    {
                        BuyerTaxId = inv.Nmmst,
                        InvoiceNumber = inv.Shdon,
                        OrgId = WorkingOrg,
                        Content = invoiceDetailResponse.Data.ToString(),
                        InvoiceDate = DateTime.UtcNow.ToLocalTime(),
                        Message = ex.Message,
                        Period = $"{from} - {to}"
                    });
                }
                finally
                {
                    if (deserializedList.Count == 10)
                    {
                        insertedCount += await soldInvoiceDetailRepository.InsertManyInvoiceAsync(deserializedList);
                        logger.LogInfoFormatted($"A batch of 10 invoices has been successfully inserted. Total {insertedCount} invoices have been saved.");
                        deserializedList.Clear();
                    }
                }
            }
        }

        //Insert any remaining records
        if (deserializedList.Count > 0)
        {
            insertedCount += await soldInvoiceDetailRepository.InsertManyInvoiceAsync(deserializedList);
            logger.LogInfoFormatted($"Inserted remaining {deserializedList.Count} invoices. Total {insertedCount} invoices have been saved.");
        }

        return new ResponseEntity
        {
            Success = true,
            Code = "200",
            Message = $"Đã tải xong {insertedCount}/{countFromResponse} hóa đơn.",
            Data = new
            {
                Total = countFromResponse,
                Inserted = insertedCount, 
                Duplication = duplicateCount,
                ErrorCount = errorCount,
                Errors = errorInvoiceList
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
        return await soldInvoiceDetailRepository.InvoiceExist(filter);
    }
}
