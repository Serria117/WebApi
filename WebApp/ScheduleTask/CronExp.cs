namespace WebApp.ScheduleTask;

public struct CronExp
{
    /// <summary>
    /// Every day at midnight.
    /// </summary>
    public const string EveryDayAt00 = "0 0 0 1/1 * ? *";
    
    /// <summary>
    /// Every first day of month.
    /// </summary>
    public const string FirstDayOfMonth = "0 0 0 1 1/1 ? *";
}