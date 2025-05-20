using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;


namespace WebApp.Services.CommonService;

public static partial class StringService
{
    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex SpaceRegex();
    
    [GeneratedRegex("\\p{IsCombiningDiacriticalMarks}+")]
    private static partial Regex UnsignRegex();

    /// <summary>
    /// Removes unnecessary spaces from the input string by trimming leading and trailing whitespace
    /// and replacing consecutive spaces within the string with a single space.
    /// </summary>
    /// <param name="str">The input string from which spaces need to be removed or normalized.</param>
    /// <returns>A string with normalized spaces or null if the input string is null or empty.</returns>
    public static string? RemoveSpace(this string? str)
    {
        return string.IsNullOrEmpty(str) ? null : SpaceRegex().Replace(str.Trim(), " ");
    }
    
    public static decimal ToDecimal(this string text)
    {
        return decimal.TryParse(text, out var result) ? result : 0M;
    }

    public static int ToInt(this string? text)
    {
        return int.TryParse(text, out var result) ? result : 0;
    }
    
    /// <summary>
    /// Removes diacritical marks (accents) from the input string and replaces certain specific characters with their non-accented equivalents.
    /// </summary>
    /// <param name="s">The input string from which diacritical marks and specific characters will be removed.</param>
    /// <returns>A string with diacritical marks removed and certain characters replaced.</returns>
    public static string UnSign(this string s)
    {
        var regex = UnsignRegex();
        var temp = s.Normalize(NormalizationForm.FormD);
        return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').RemoveSpace()!;
    }

    public static string? GetXmlNodeValue(this XDocument doc, string nodeName)
    {
        return doc.Descendants()
          .FirstOrDefault(e => e.Name.LocalName == nodeName)?
          .Value;
    }
    
    public static long GetXmlNodeValueAsLong(this XDocument doc, string nodeName)
    {
        return long.TryParse(doc.GetXmlNodeValue(nodeName), out var value) ? value : 0;
    }

    public static long GetValueFromElementAsLong(this XElement e, string nameSpace, XNamespace nodeName)
    {
        return long.TryParse(e.Element(nameSpace + nodeName)?.Value, out var value) ? value : 0;
    }

    /// <summary>
    /// Converts a string representation of a date to a nullable DateTime object.
    /// </summary>
    /// <param name="dateString">The input string representing a date in "dd/MM/yyyy" or "yyyy-MM-dd" format.</param>
    /// <returns>A nullable DateTime object if the conversion succeeds; otherwise, null.</returns>
    public static DateTime? ToDateTime(this string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return null;
        }

        const DateTimeStyles style = DateTimeStyles.None;
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Thử định dạng dd/MM/yyyy
        if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", culture, style, out DateTime dateTime))
        {
            return dateTime;
        }

        // Thử định dạng yyyy-MM-dd
        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", culture, style, out dateTime))
        {
            return dateTime;
        }

        // Nếu không khớp với định dạng nào, trả về null
        return null;
    }

    /// <summary>
    /// Parses a string to extract a year and period value based on a specific format.
    /// The input string is expected to be in the format "Period/Year" (e.g., "12/2023").
    /// If the format is invalid, appropriate defaults (0 or missing values) will be returned.
    /// </summary>
    /// <param name="str">The input string containing the period and year information, separated by a "/".</param>
    /// <returns>A tuple containing the year as an integer and the period as an integer. Returns (0, 0) if the input is null or invalid.</returns>
    public static (int Year, int Period) GetPeriod(this string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return (0, 0);
        }

        var splitted = str.Split("/");
        return splitted.Length > 1 
            ? (splitted[1].ToInt(), splitted[0].ToInt()) 
            : (splitted[0].ToInt(), 0);
    }

    public static Guid ToGuid(this string? str)
    {
        return Guid.TryParse(str, out var result) ? result : Guid.Empty;
    }
}