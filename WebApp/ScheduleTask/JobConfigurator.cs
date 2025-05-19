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
        services.AddQuartz(quartzConfig =>
        {
            // Add job keys:
            var clearTokenJob = new JobKey("ClearOldRefreshTokenJob");
            var clearOldLogJob = new JobKey("CleanOldUserLogJob");
            
            // Add jobs and triggers:
            quartzConfig.AddJob<ClearOldRefreshTokenJob>(opts => opts.WithIdentity(clearTokenJob));
            quartzConfig.AddTrigger(opts => opts
                                 .ForJob(clearTokenJob)
                                 .WithIdentity("ClearOldRefreshTokenJob-trigger#1")
                                 .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0)
                                                                  .InTimeZone(vnTimeZone)) // Thực hiện hàng ngày lúc 0h0m
                                 .StartNow()
            );

            quartzConfig.AddJob<CleanOldUserLogJob>(ops => ops.WithIdentity(clearOldLogJob));
            quartzConfig.AddTrigger(opts => opts
                                 .ForJob(clearOldLogJob)
                                 .WithIdentity("CleanOldUserLogJob-trigger#2")
                                 .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(1, 0, 0)
                                                                  .InTimeZone(vnTimeZone))
                                 .StartNow()
            );

            // Đăng ký các job khác ở đây...
        });


        // Thêm hosted service để chạy các job đã đăng ký
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        return services;
    }
}