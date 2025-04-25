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
        services.AddQuartz(q =>
        {
            // Múi giờ Việt Nam
            var vnTimeZone = TZConvert.GetTimeZoneInfo("Asia/Ho_Chi_Minh");

            // Đăng ký job + trigger
            var jobKey = new JobKey("ClearOldRefreshTokenJob");
            
            q.AddJob<ClearOldRefreshTokenJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                                 .ForJob(jobKey)
                                 .WithIdentity("ClearOldRefreshTokenJob-trigger#1")
                                 .WithCronSchedule(CronExp.EveryDayAt00, x => x.InTimeZone(vnTimeZone))  // 0h mỗi ngày
            );
            
        });
        // Đăng ký các job khác ở đây...
        
        // Thêm hosted service để chạy các job đã đăng ký
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        return services;
    }
}