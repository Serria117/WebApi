using System.Diagnostics;
using WebApp.Core.DomainEntities;
using WebApp.Queues;
using WebApp.Repositories;

namespace WebApp.Services.BackgroundServices;

public class UserLogBackgroundService(IUserLogQueue logQueue,
                                      ILogger<UserLogBackgroundService> logger,
                                      IServiceScopeFactory scopeFactory) : BackgroundService
{
    private const int BatchSize = 100; // Số lượng log tối đa ghi cùng lúc

    private readonly TimeSpan
        _dequeueInterval = TimeSpan.FromMilliseconds(500); // Thời gian chờ giữa các lần kiểm tra queue nếu rỗng

    private TimeSpan _currentBackoff = TimeSpan.FromSeconds(1);
    private const int MaxBackoffSeconds = 60;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Thực hiện vòng lặp vô hạn cho đến khi token cancellation được yêu cầu
        while (!stoppingToken.IsCancellationRequested)
        {
            var logToSave = new List<UserLog>();

            // Lấy ra các log từ queue và thêm vào danh sách để lưu
            while (logQueue.TryDequeue(out var log) && logToSave.Count < BatchSize)
            {
                logToSave.Add(log);
            }

            if (logToSave.Count > 0)
            {
                using var scope = scopeFactory.CreateScope();
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30-second timeout

                    logger.LogInformation("Saving {count} logs to database", logToSave.Count);
                    var logRepository = scope.ServiceProvider.GetRequiredService<IAppRepository<UserLog, Guid>>();

                    await logRepository.CreateManyAsync(logToSave, cts.Token);
                    
                    _currentBackoff = TimeSpan.FromSeconds(1);
                    
                    var stopwatch = Stopwatch.StartNew();
                    logger.LogInformation(
                        "Queue processing metrics: processed={count}, queueSize={queueSize}, processingTime={time}ms",
                        logToSave.Count, logQueue.Count, stopwatch.ElapsedMilliseconds);
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning("Database operation timed out");
                    foreach (var log in logToSave)
                    {
                        logQueue.Enqueue(log);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("Error creating user's log. {message}", ex.Message);
                    logger.LogInformation(ex.StackTrace);
                    
                    if (_currentBackoff < TimeSpan.FromSeconds(MaxBackoffSeconds))
                        _currentBackoff = TimeSpan.FromSeconds(Math.Min(MaxBackoffSeconds, _currentBackoff.TotalSeconds * 2));
                    await Task.Delay(_currentBackoff, stoppingToken);
                    
                    //Add back the unfinished logs to the queue
                    foreach (var log in logToSave)
                    {
                        logQueue.Enqueue(log);
                    }
                }
            }
            else
            {
                // Nếu queue rỗng, chờ một chút trước khi kiểm tra lại
                await Task.Delay(_dequeueInterval, stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("User Log Background Service is performing final cleanup.");

        while (logQueue.Count > 0 && !stoppingToken.IsCancellationRequested)
        {
            var logToSave = new List<UserLog>();
            while (logToSave.Count < BatchSize && logQueue.TryDequeue(out var log))
            {
                logToSave.Add(log);
            }

            if (logToSave.Count <= 0)
            {
                continue;
            }

            using var scope = scopeFactory.CreateScope();
            var logRepository = scope.ServiceProvider.GetRequiredService<IAppRepository<UserLog, Guid>>();
            try
            {
                await logRepository.CreateManyAsync(logToSave, CancellationToken.None);
                logger.LogInformation("Saved {Count} remaining user logs during shutdown.", logToSave.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving remaining user logs during shutdown.");
                // Tùy chọn: Lưu các log này vào file hoặc nơi khác nếu không thể ghi vào DB
            }
            // Không cần Task.Delay ở đây vì muốn xử lý nhanh nhất có thể khi shutdown
        }

        await base.StopAsync(stoppingToken);
        logger.LogInformation("User Log Background Service stopped.");
    }
}