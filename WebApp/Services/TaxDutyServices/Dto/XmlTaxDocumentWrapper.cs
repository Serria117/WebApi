using System.Xml.Linq;
using WebApp.Core.DomainEntities.Accounting.TaxDeclarations;
using WebApp.Utils;

namespace WebApp.Services.TaxDutyServices.Dto;
/// <summary>
/// Wrapper for XML tax document, providing convenient access to its properties.
/// </summary>
/// <param name="document"></param>
public class XmlTaxDocumentWrapper(XDocument document)
{
    public XDocument Document { get; set; } = document;
    public string Code => Document.Descendants().First(x => x.Name.LocalName == "maTKhai").Value;
    public string TaxId => Document.Descendants().First(x => x.Name.LocalName == "mst").Value;
    public string Period => Document.Descendants().First(x => x.Name.LocalName == "kyKKhai").Value;
    public string PeriodType => Document.Descendants().First(x => x.Name.LocalName == "kieuKy").Value;
    public string ReportType => Document.Descendants().First(x => x.Name.LocalName == "loaiTKhai").Value;
    public int Count => Document.Descendants().First(x => x.Name.LocalName == "soLan").Value.ToInt();
    public string IssueDate => Document.Descendants().First(x => x.Name.LocalName == "ngayLapTKhai").Value;
    public string? SignDate => Document.Descendants().FirstOrDefault(x => x.Name.LocalName == "ngayKy")?.Value;
    public string? FromDate => Document.Descendants().FirstOrDefault(x => x.Name.LocalName == "kyKKhaiTuNgay")?.Value;
    public string? ToDate => Document.Descendants().FirstOrDefault(x => x.Name.LocalName == "kyKKhaiDenNgay")?.Value;
    public string? TaxAuthorityCode => Document.Descendants().FirstOrDefault(x => x.Name.LocalName == "maCQTNoiNop")?.Value;
    
    public string Content => Document.ToString();

    public TaxDutyXmlDoc CreateXmlRecord()
    {
        return new TaxDutyXmlDoc
        {
            TaxId = TaxId,
            Period = Period,
            PeriodType = PeriodType,
            Content = Content,
            IssueDate = IssueDate.ToDateTime(),
            SubmissionCount = Count,
        };
    }

}