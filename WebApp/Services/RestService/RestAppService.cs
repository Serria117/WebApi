using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Newtonsoft.Json;
using Polly;
using RestSharp;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.NotificationService;
using WebApp.Services.RestService.Dto;
using WebApp.Services.RestService.Dto.SoldInvoice;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services.RestService;

public interface IRestAppService
{
    Task<ResponseEntity> Authenticate(InvoiceLoginModel login);
    Task<CaptchaModel?> GetCaptcha();

    Task<ResponseEntity>
        GetPurchaseInvoiceListInRange(string token, string from, string to, int[]? invoiceTypes = null);

    /// <summary>
    /// Attempt to get an invoice's detail of goods sold
    /// </summary>
    /// <param name="token">Bearer token to use in the request to hoadondientu service</param>
    /// <param name="invoiceModel"></param>
    /// <returns>A response object containing the result of the request</returns>
    Task<ResponseEntity> GetPurchaseInvoiceDetail(string token, InvoiceModel invoiceModel);

    Task<ResponseEntity> GetSoldInvoiceInRangeAsync(string token, string from, string to);

    /// <summary>
    /// Get a specific invoice's detail of goods sold
    /// </summary>
    /// <param name="token">jwt token to use in the request</param>
    /// <param name="invoice">The invoice object to get detail</param>
    /// <returns>The ResponseBase object containing the result of the request</returns>
    Task<ResponseEntity> GetSoldInvoiceDetail(string token, SoldInvoiceModel invoice);
}

public class RestBaseAppService(IRestClient restClient,
                                RestSharpSetting setting,
                                ILogger<RestBaseAppService> logger,
                                INotificationAppService notificationService,
                                IAppRepository<InvoiceServiceToken, string> invoiceServiceTokenRepository,
                                IUserManager userManager)
    : BaseAppService(userManager), IRestAppService
{
    private const int MaxRetries = 5;
    private const int DelaySeconds = 30;

    #region Authentication

    public async Task<CaptchaModel?> GetCaptcha()
    {
        var request = new RestRequest("/captcha", Method.Get);
        request.AddHeader("Cookie", setting.Cookie);
        var response = await restClient.ExecuteAsync<CaptchaModel>(request);
        if (response is not { IsSuccessStatusCode: true, Data: not null }) return null;
        var data = response.Data;
        return data;
    }

    public async Task<ResponseEntity> Authenticate(InvoiceLoginModel login)
    {
        var request = new RestRequest("/security-taxpayer/authenticate", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Cookie", setting.AuthCookie);
        var requestBody = new
        {
            username = login.Username,
            password = login.Password,
            cvalue = login.Cvalue,
            ckey = login.Ckey
        };
        request.AddBody(requestBody);
        //Try to find the token in db first, if it exists and valid, retrieve it and do not call the API to save resources
        var foundToken = await invoiceServiceTokenRepository.Find(t => t.TaxId == requestBody.username)
                                                            .FirstOrDefaultAsync();
        if (foundToken is not null)
        {
            var token = foundToken.Token;
            var jwtHandler = new JwtSecurityTokenHandler();
            var expiredTime = jwtHandler.ReadJwtToken(token).ValidTo;
            //Check if the token is expired
            if (expiredTime > DateTime.UtcNow)
            {
                logger.LogInformation("found valid token in database");
                return ResponseEntity.OkResult(new InvoiceAuthenticationResponse
                {
                    Token = token ?? string.Empty,
                    Success = true
                });
            }

            logger.LogInformation("found expired token in database, will try to refresh it");
            var response = await restClient.ExecuteAsync<InvoiceAuthenticationResponse>(request);
            if (response is { IsSuccessful: true, Data: not null })
            {
                var newToken = response.Data;
                foundToken.Token = newToken.Token;
                await invoiceServiceTokenRepository.UpdateAsync(foundToken);
                return ResponseEntity.OkResult(response.Data);
            }

            return new ResponseEntity
            {
                Success = false,
                Message = response.Data?.Message,
                Data = response.Data
            };
        }
        else
        {
            logger.LogInformation("No token found in database, will try to authenticate");
            var response = await restClient.ExecuteAsync<InvoiceAuthenticationResponse>(request);
            if (response is { IsSuccessful: true, Data: not null })
            {
                await invoiceServiceTokenRepository.CreateAsync(new InvoiceServiceToken
                {
                    Id = Ulid.NewUlid().ToString(),
                    TaxId = login.Username,
                    Token = response.Data.Token
                });
                return ResponseEntity.OkResult(response.Data);
            }

            Console.WriteLine(response.Content);
            return new ResponseEntity
            {
                Success = false,
                Message = response.Data?.Message,
                Data = response.Data
            };
        }
    }

    #endregion

    #region SOLD INVOICE METHODS

    public async Task<ResponseEntity> GetSoldInvoiceInRangeAsync(string token, string from, string to)
    {
        List<SoldInvoiceModel> invoicesList = [];

        var fromValue = DateTime.ParseExact(from, "yyyy-MM-dd",
                                            CultureInfo.InvariantCulture, DateTimeStyles.None);
        var toValue = DateTime.ParseExact(to, "yyyy-MM-dd",
                                          CultureInfo.InvariantCulture, DateTimeStyles.None);

        var dateRanges = CommonFn.SplitDateRange(fromValue, toValue);

        List<string> endpoints =
        [
            "/query/invoices/sold",
            "/sco-query/invoices/sold"
        ];
        var countFromResponse = 0;
        foreach (string endpoint in endpoints)
        {
            Console.WriteLine($"Executing enpoint: {endpoint}");
            Console.WriteLine("---------------------------------");
            foreach (var dateRange in dateRanges)
            {
                Console.WriteLine(
                    $"Getting purchase invoices from {dateRange.GetFromDate()} to {dateRange.GetToDate()}");
                var pageCount = 1;
                var invoiceCount = 0;
                await Task.Delay(2000);
                var result =
                    await GetSoldInvoiceFromService(token, endpoint, dateRange.GetFromDate(), dateRange.GetToDate());
                await notificationService
                    .SendAsync(UserId, HubName.InvoiceMessage,
                               $"Tải thông tin hóa đơn bán ra - Từ ngày: {dateRange.GetFromDate()} " +
                               $"đến ngày {dateRange.GetToDate()}\n Trang: {pageCount}");

                if (result == null || result.Datas.Count == 0)
                {
                    logger.LogInformation("Found no invoice from {dateRange}", dateRange.ToString());
                    await notificationService
                        .SendAsync(UserId, HubName.InvoiceMessage,
                                   $"Không tìm thấy hóa đơn bán ra trong khoảng thời gian\n " +
                                   $"từ ngày {dateRange.GetFromDate()} đến ngày {dateRange.GetToDate()}");
                    continue;
                }

                countFromResponse += result.Datas.Count;
                invoicesList.AddRange(result.Datas);
                Console.WriteLine($"Page {pageCount} has: {result.Datas.Count} invoices");
                invoiceCount += invoicesList.Count;
                if (result.State == null)
                {
                    logger.LogInformation("Found {total} invoices from {range}. No more pages left",
                                          invoicesList.Count, dateRange.ToString());
                    await notificationService
                        .SendAsync(UserId, HubName.InvoiceMessage,
                                   $"Tìm thấy {invoiceCount} hóa đơn từ ngày " +
                                   $"{dateRange.GetFromDate()} đến ngày {dateRange.GetToDate()}");
                    continue;
                }

                var nextState = result.State;

                while (true)
                {
                    if (nextState is not null)
                    {
                        await Task.Delay(1000); //delay before each call to avoid rejection
                        pageCount++;
                        var nextResult = await GetSoldInvoiceFromService(token, endpoint,
                                                                         dateRange.GetFromDate(),
                                                                         dateRange.GetToDate(),
                                                                         state: nextState);
                        if (nextResult == null) break;
                        invoicesList.AddRange(nextResult.Datas);
                        Console.WriteLine($"Page {pageCount} has: {nextResult.Datas.Count} invoices");
                        invoiceCount += invoicesList.Count;
                        countFromResponse += nextResult.Datas.Count;
                        if (nextResult.State == null) break;
                        nextState = nextResult.State;
                    }
                    else
                    {
                        break;
                    }
                }

                logger.LogInformation("Found {total} invoices from {from} to {to}. No more pages left",
                                      invoicesList.Count, dateRange.GetFromDate(), dateRange.GetToDate());
                await notificationService
                    .SendAsync(UserId, HubName.InvoiceMessage,
                               $"Tìm thấy {invoiceCount} hóa đơn từ ngày" +
                               $" {dateRange.GetFromDate()} đến ngày {dateRange.GetToDate()}");
            }
        }

        Console.WriteLine($"InvoiceList count: {invoicesList.Count}");
        Console.WriteLine($"Count from response: {countFromResponse}");
        return new ResponseEntity
        {
            Code = "200",
            Success = true,
            TotalCount = countFromResponse,
            Message = $"Found {countFromResponse} invoices in total",
            Data = invoicesList
        };
    }

    /// <summary>
    /// Queries sold invoices from the service within a specified date range, optionally continuing from a previous state.
    /// </summary>
    /// <param name="token">Bearer token used for authentication with the service</param>
    /// <param name="endpoint">The URL</param>
    /// <param name="from">Start date of the range for fetching sold invoices, formatted as a string</param>
    /// <param name="to">End date of the range for fetching sold invoices, formatted as a string</param>
    /// <param name="state">Optional parameter representing the pagination state for fetching subsequent data, if available</param>
    /// <returns>A SoldInvoiceResponseModel object containing the results of the request or null if the request fails</returns>
    private async Task<SoldInvoiceResponseModel?> GetSoldInvoiceFromService(string token, string endpoint,
                                                                            string from, string to,
                                                                            string? state = null)
    {
        var request = new RestRequest(endpoint, Method.Get);
        request.AddHeader("Cookie", setting.Cookie);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("sort", "tdlap:desc");
        request.AddQueryParameter("size", 50);
        request.AddQueryParameter("search", $"tdlap=ge={from}T00:00:00;tdlap=le={to}T23:59:59");
        if (state is not null)
        {
            request.AddQueryParameter("state", state);
        }

        var response = await restClient.ExecuteAsync<SoldInvoiceResponseModel>(request);
        if (response.IsSuccessful)
        {
            logger.LogInformation("Successfully retrieved {count}  invoices from {From} to {To}",
                                  response.Data!.Datas.Count, from, to);

            return response.Data;
        }

        logger.LogInfoFormatted("Failed to deserialize response.");

        //if the response is not successful, try to deserialize the content
        var json = response.Content;
        Console.WriteLine("WARNING - Undeserializable content: " + json);
        if (json is not null && response.StatusCode == HttpStatusCode.OK)
        {
            logger.LogInfoFormatted("Attempting to deserialize valid content");
            var data = JsonConvert.DeserializeObject<SoldInvoiceResponseModel>(json!);
            Console.WriteLine(response.ErrorMessage);
            return data;
        }

        return default;
    }

    public async Task<ResponseEntity> GetSoldInvoiceDetail(string token, SoldInvoiceModel invoice)
    {
        List<string> endpoints = ["/query/invoices/detail", "/sco-query/invoices/detail"];

        var endpoint = invoice switch
        {
            { Ttxly: 8 } => endpoints[1],
            _ => endpoints[0]
        };
        var request = new RestRequest(endpoint, Method.Get);
        request.AddHeader("Cookie", setting.Cookie);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddParameter("nbmst", invoice.Nbmst);
        request.AddParameter("khhdon", invoice.Khhdon);
        request.AddParameter("shdon", invoice.Shdon);
        request.AddParameter("khmshdon", invoice.Khmshdon);

        await Task.Delay(800); //delay before each call to avoid rejection
        var response = await restClient.ExecuteAsync<SoldInvoiceDetail>(request);


        var statusCode = response.StatusCode;
        var retryCount = 0;
        const int delay = 30;
        while (statusCode == HttpStatusCode.TooManyRequests)
        {
            if (retryCount > 5) break;
            logger.LogWarning("Too many requests. Retrying after {Delay} seconds...", delay);
            await notificationService.SendAsync(UserId, "429",
                                                $"Too many requests. Retry {retryCount + 1}/5 after {delay} seconds...");
            await Task.Delay(delay * 1000);
            response = await restClient.ExecuteAsync<SoldInvoiceDetail>(request);
            statusCode = response.StatusCode;
            retryCount++;
            logger.LogWarning("Retry {RetryCount} completed", retryCount);
        }

        if (retryCount > 5)
        {
            return new ResponseEntity
            {
                Code = "429",
                Success = false,
                Message = "429 - Too many request",
                Data = null
            };
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning("Something wrong with the response {statuscode}", response.StatusCode.ToString());
            //logger.LogError("The content of error response:\n {content}", response.Content);
            return new ResponseEntity
            {
                Success = false,
                Message = $"{response.StatusCode.ToString()} - {response.Content}",
                Data = $"Failed to retrieve invoice number: [{invoice.Shdon}]"
            };
        }

        if (response is { Content: not null, Data: null })
        {
            return new ResponseEntity
            {
                Code = "99", //Mark this case as success but content is empty
                Success = true,
                Message =
                    "99 - auto-deserialize failed. Invoice object will be store as string and attempted to be deserialized using JSON converter",
                Data = response.Content,
            };
        }

        //Console.WriteLine($"{response.Content} successfully retrieved");
        return ResponseEntity.OkResult(response.Data!);
    }

    #endregion

    #region PURCHASE INVOICE METHODS

    public async Task<ResponseEntity> GetPurchaseInvoiceListInRange(string token,
                                                                    string from,
                                                                    string to,
                                                                    int[]? invoiceTypes = null)
    {
        try
        {
            logger.LogInformation("Starting Get Invoice List at {time}", DateTime.Now.ToLocalTime());
            long? countFromResponse = 0;
            invoiceTypes ??= [5, 6, 8];
            int[] types = [.. invoiceTypes];
            List<InvoiceModel> invoicesList = [];
            var fromValue = DateTime.ParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
            var toValue = DateTime.ParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);

            if (fromValue > toValue) throw new InvalidDataException("[From date] can not be greater than [To date]");

            var dateRanges = CommonFn.SplitDateRange(fromValue, toValue);

            foreach (var type in types)
            {
                var endpoint = type switch
                {
                    8 => "/sco-query/invoices/purchase",
                    _ => "/query/invoices/purchase"
                };
                var displayType = type switch
                {
                    5 => "Hóa đơn được cấp mã",
                    6 => "Hóa đơn không cấp mã",
                    8 => "Hóa đơn từ máy tính tính tiền",
                    _ => string.Empty
                };
                foreach (var dateRange in dateRanges)
                {
                    var pageCount = 1;
                    await Task.Delay(1000);
                    await notificationService.SendAsync(UserId, HubName.InvoiceMessage,
                                                        $"Tải thông tin {displayType} " +
                                                        $"- Từ ngày: {dateRange.GetFromDate()} " +
                                                        $"đến ngày {dateRange.GetToDate()}\n " +
                                                        $"Trang: {pageCount}");
                    Console.WriteLine(
                        $"Get invoice type {type} of page {pageCount} - from {dateRange.GetFromDate()} to {dateRange.GetToDate()}");
                    try
                    {
                        var invoiceResponse = await GetPurchaseInvoiceFromService(token, endpoint,
                            dateRange.GetFromDate(),
                            dateRange.GetToDate(), type);
                        if (invoiceResponse == null) continue;
                        countFromResponse += invoiceResponse.Total; //Total count of invoices from the response
                        invoicesList.AddRange(invoiceResponse.Datas);
                        if (invoiceResponse.State == null) continue;
                        var nextState = invoiceResponse.State;
                        while (true)
                        {
                            await Task.Delay(1000); //delay before each call to avoid rejection
                            pageCount++;
                            var nextResult = await GetPurchaseInvoiceFromService(token, endpoint,
                                dateRange.GetFromDate(),
                                dateRange.GetToDate(),
                                type, nextState);
                            await notificationService.SendAsync(UserId, HubName.InvoiceMessage,
                                                                $"Tải thông tin {displayType} - " +
                                                                $"Từ ngày: {dateRange.GetFromDate()} " +
                                                                $"đến ngày {dateRange.GetToDate()}\n " +
                                                                $"Trang: {pageCount}");
                            Console.WriteLine(
                                $"Get invoice type {type} of page {pageCount} - " +
                                $"from {dateRange.GetFromDate()} to {dateRange.GetToDate()}");
                            if (nextResult == null) break;
                            invoicesList.AddRange(nextResult.Datas);
                            if (nextResult.State == null) break;
                            nextState = nextResult.State;
                        }
                    }
                    catch (RequestCanceledException)
                    {
                        logger.LogWarning("Request was canceled due to timeout at {time}", DateTime.Now.ToLocalTime());
                        continue;
                    }
                    catch (RequestFailedException e)
                    {
                        logger.LogWarning("Request failed at {time}", DateTime.Now.ToLocalTime());
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
            }

            logger.LogInformation("Finished getting Invoice List at: {time}", DateTime.Now.ToLocalTime());

            return new ResponseEntity
            {
                Code = "200",
                Success = true,
                TotalCount = countFromResponse,
                Message = $"Tìm thấy {countFromResponse} hóa đơn.",
                Data = invoicesList
            };
        }
        catch (Exception e)
        {
            logger.LogWarning("Interupted with error [{err}] at {time}", e.Message, DateTime.Now.ToLocalTime());
            return ResponseEntity.Error(e.Message);
        }
    }

    /// <summary>
    /// Get the list of purchase invoice in date range that limited by the external service.
    /// The invoices in the list has no goods detail
    /// </summary>
    /// <param name="token">Authorization token to access external service</param>
    /// <param name="endpoint"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="type"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<InvoiceResponseModel?> GetPurchaseInvoiceFromService(string token, string endpoint,
                                                                            string from, string to,
                                                                            int type, string? state = null)
    {
        var request = new RestRequest(endpoint, Method.Get);
        //set time-out for the request, after the given seconds, the request will be cancelled
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        request.AddHeader("Cookie", setting.Cookie);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("sort", "tdlap:desc");
        request.AddQueryParameter("size", 50);
        request.AddQueryParameter("search", $"tdlap=ge={from}T00:00:00;tdlap=le={to}T23:59:59;ttxly=={type}");

        if (state is not null)
        {
            request.AddQueryParameter("state", state);
        }

        try
        {
            var response = await restClient.ExecuteAsync<InvoiceResponseModel>(request, cts.Token);
            if (!response.IsSuccessful) throw new RequestFailedException($"Error: {response.Content}");
            return response.Data;
        }
        catch (TaskCanceledException e) when (cts.IsCancellationRequested)
        {
            logger.LogErrorFormatted(exception: e);
            throw new RequestCanceledException("Request was canceled due to timeout.");
        }
        catch (RequestFailedException e)
        {
            logger.LogErrorFormatted(exception: e);
            throw;
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            throw new RequestFailedException("Failed to complete the request");
        }
    }

    public async Task<ResponseEntity> GetPurchaseInvoiceDetail(string token, InvoiceModel invoiceModel)
    {
        var invoice = invoiceModel.ToDisplayModel();
        var endpoint = invoice.InvoiceTypeNumber switch
        {
            8 => "/sco-query/invoices/detail",
            _ => "/query/invoices/detail"
        };
        var request = new RestRequest(endpoint, Method.Get);
        request.AddHeader("Cookie", setting.Cookie);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("nbmst", invoice.SellerTaxCode);
        request.AddQueryParameter("khhdon", invoice.InvoiceNotation);
        request.AddQueryParameter("shdon", invoice.InvoiceNumber);
        request.AddQueryParameter("khmshdon", invoice.InvoiceGroupNotation.ToString());

        Console.WriteLine(
            $"Extracting {invoice.InvoiceNumber} - {invoice.SellerTaxCode} " +
            $"- {invoice.CreationDate:dd/MM/yyyy} " +
            $"- {invoice.SellerName}");
        Console.WriteLine($"Invoice status: {invoice.StatusNumber} - {invoice.Status}");
        Console.WriteLine($"Invoice type: {invoice.InvoiceType}");

        await Task.Delay(800); //delay before each call to avoid rejection

        var response = await restClient.ExecuteAsync<InvoiceDetailModel>(request);

        var statusCode = response.StatusCode;
        var retryCount = 0;
        const int delay = 30;
        while (statusCode == HttpStatusCode.TooManyRequests)
        {
            if (retryCount > 5) break;
            Console.WriteLine($"Too many requests. Retrying after {delay} seconds...");
            await notificationService.SendAsync(UserId, "429",
                                                $"Hệ thống hóa đơn điện tử đang quá tải. " +
                                                $"Hệ thống đang cố gắng thử lại {retryCount + 1}/5 sau {delay} giây...");
            await Task.Delay(delay * 1000);
            response = await restClient.ExecuteAsync<InvoiceDetailModel>(request);
            statusCode = response.StatusCode;
            retryCount++;
            Console.WriteLine($"Retry {retryCount} completed");
        }

        if (retryCount > 5)
        {
            return new ResponseEntity
            {
                Code = InvoiceDetailStatus.TooManyRequest.ToString(),
                Success = false,
                Message = "Hệ thống không thể truy cập ứng dụng hóa đơn điện tử. Hãy thử lại sau!",
                Data = null
            };
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning("Something wrong with the response {}", response.StatusCode);
            return new ResponseEntity
            {
                Success = false,
                Code = InvoiceDetailStatus.Failed.ToString(),
                Message = $"{response.StatusCode} - {response.Content}",
                Data = $"Failed to retrieve invoice [{invoice.InvoiceNumber}] of [{invoice.SellerName}]"
            };
        }

        if (response is { Content: not null, Data: null })
        {
            return new ResponseEntity
            {
                Success = true,
                Code = InvoiceDetailStatus.Undeserializable.ToString(),
                Message = """
                          99 - auto-deserialize failed. 
                          Invoice object will be store as string and attempted to be deserialized using JSON converter
                          """,
                Data = response.Content,
            };
        }

        return new ResponseEntity
        {
            Data = response.Data,
            Success = true,
            Code = InvoiceDetailStatus.Success.ToString(),
        };
    }

    #endregion

    private async Task<RestResponse<T>> ExecuteWithRetryAsync<T>(RestRequest request)
    {
        var retryPolicy = Policy<RestResponse<T>>
                          .Handle<HttpRequestException>()
                          .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                          .WaitAndRetryAsync(retryCount: MaxRetries,
                                             retryAttempt => TimeSpan.FromSeconds(DelaySeconds),
                                             onRetry: async void (exception, timeSpan, retryCount, context) =>
                                             {
                                                 try
                                                 {
                                                     logger.LogWarning(
                                                         "Attempt {RetryCount} - Retrying after {Delay} seconds...",
                                                         retryCount, DelaySeconds);
                                                     await notificationService.SendAsync(UserId, "429",
                                                         $"Too many requests. Retry {retryCount}/{MaxRetries} after {DelaySeconds} seconds...");
                                                 }
                                                 catch (Exception e)
                                                 {
                                                     logger.LogError(e, "Error sending notification.");
                                                 }
                                             });

        return await retryPolicy.ExecuteAsync(() => restClient.ExecuteAsync<T>(request));
    }
}