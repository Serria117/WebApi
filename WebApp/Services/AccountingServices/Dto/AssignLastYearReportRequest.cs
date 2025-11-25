using WebApp.Utils;

namespace WebApp.Services.AccountingServices.Dto;

public class AssignLastYearReportRequest
{
    public string ReportId { get; set => field = value.TrimSpace(); } = string.Empty;
    public string LastYearReportId { get; set => field = value.TrimSpace(); } = string.Empty;
}