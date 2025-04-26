using Quartz;
using TimeZoneConverter;

namespace WebApp.ScheduleTask;

public static class JobConfigurator
{
    /// <summary>
    /// Add Quartz jobs to the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        var vnTimeZone = TZConvert.GetTimeZoneInfo("Asia/Ho_Chi_Minh");
        services.AddQuartz(q =>
        {
            // Đăng ký job + trigger
            var clearTokenJob = new JobKey("ClearOldRefreshTokenJob");
            q.AddJob<ClearOldRefreshTokenJob>(opts => opts.WithIdentity(clearTokenJob));
            q.AddTrigger(opts => opts
                                 .ForJob(clearTokenJob)
                                 .WithIdentity("ClearOldRefreshTokenJob-trigger#1")
                                 .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0).InTimeZone(vnTimeZone)) // Thực hiện hàng ngày lúc 0h0m
                                 .StartNow() // Bắt đầu ngay lập tức khi ứng dụng khởi động
            );
            
        });
        // Đăng ký các job khác ở đây...
        

        // Thêm hosted service để chạy các job đã đăng ký
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        return services;
    }
}