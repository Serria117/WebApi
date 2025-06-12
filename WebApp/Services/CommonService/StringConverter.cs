using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;


namespace WebApp.Services.CommonService;

/// <summary>
/// Static class providing various string manipulation and conversion methods.
/// Includes methods for XML document handling.
/// </summary>
public static partial class StringConverter
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
    
     /// <summary>
    /// Determines whether the specified string is null or an empty string ("").
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
    /// <remarks>This is an extension method style replacement for string.IsNullOrEmpty()</remarks>
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Determines whether the specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns>true if the value parameter is null, empty, or consists only of white-space characters; otherwise, false.</returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Converts a percentage string to a decimal value.
    /// </summary>
    /// <param name="value">The input string representing a percentage (e.g., "50%").</param>
    /// <returns>
    /// A decimal value representing the percentage divided by 100.
    /// Returns 0 if the input string is null, empty, or cannot be parsed.
    /// </returns>
    public static decimal Percent(this string value)
    {
        if(value.IsNullOrEmpty()) return 0;
        value = value.Replace("%", "").Trim();
        return decimal.TryParse(value, out var result) ? result / 100 : 0;
    }

    /// <summary>
    /// Converts the specified string to an integer.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted integer, or the default value if the conversion fails.</returns>
    public static int ToInt(this string? value, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Converts the specified string to a long integer.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted long integer, or the default value if the conversion fails.</returns>
    public static long ToLong(this string? value, long defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;

        return long.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Converts the specified string to a decimal number.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted decimal number, or the default value if the conversion fails.</returns>
    public static decimal ToDecimal(this string? value, decimal defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;

        return decimal.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Converts the specified string to a double-precision floating-point number.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted double-precision floating-point number, or the default value if the conversion fails.</returns>
    public static double ToDouble(this string? value, double defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;

        return double.TryParse(value, out var result) ? result : defaultValue;
    }

    /*public static DateTime ToDateTime(this string? value, DateTime defaultValue = new())
    {
        if (value.IsNullOrEmpty()) return defaultValue;
        return DateTime.TryParse(value, out var result) ? result : defaultValue;
    }*/
    
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
    /// Retrieves the value of an XML element by traversing a specified path.
    /// </summary>
    /// <param name="doc">The XML document to search within.</param>
    /// <param name="path">An array of strings representing the path to the desired element.</param>
    /// <returns>
    /// The value of the XML element at the specified path, or null if the path is invalid or the element does not exist.
    /// </returns>
    public static string? GetElementValueByPath(this XDocument doc, params string[] path)
    {
        XElement? current = doc.Root;
        foreach (var node in path)
        {
            if (current == null) return null;
            current = current.Element(node);
        }
        return current?.Value;
    }
    
    /// <summary>
    /// Retrieves the value of a child XML element by following a specified path.
    /// </summary>
    /// <param name="element">The starting XML element to traverse.</param>
    /// <param name="path">An array of strings representing the path to the desired child element.</param>
    /// <returns>
    /// The value of the final XML element in the path, or null if the path is invalid or the element does not exist.
    /// </returns>
    public static string? GetXmlChildValueByPath(this XElement? element, params string[] path)
    {
        foreach (var node in path)
        {
            if (element == null)
            {
                return null; // Return null if the path is invalid
            }
            element = element.Element(node);
        }
        return element?.Value; // Return the value of the final element
    }

    /// <summary>
    /// Retrieves a list of child XML elements by traversing a specified path and matching the child element name.
    /// </summary>
    /// <param name="doc">The XML document to search within.</param>
    /// <param name="childName">The name of the child elements to retrieve.</param>
    /// <param name="path">An array of strings representing the path to the desired parent element.</param>
    /// <returns>
    /// A list of child XML elements matching the specified name at the end of the path.
    /// Returns an empty list if the path is invalid or no matching elements are found.
    /// </returns>
    public static List<XElement> GetChildElementsByPath(this XDocument doc, 
                                                        string childName,
                                                        params string[] path)
    {
        XElement? current = doc.Root;
        foreach (var node in path)
        {
            if (current == null) return [];
            current = current.Element(node);
        }
        var children = current?.Elements(childName).ToList() ?? [];
        return children;
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

    /// <summary>
    /// Converts a string to a <see cref="Guid"/>.
    /// If the string is null or cannot be parsed as a valid <see cref="Guid"/>, returns an emtpy value of Guid.
    /// </summary>
    /// <param name="str">The input string to convert to a <see cref="Guid"/>.</param>
    /// <returns>A <see cref="Guid"/> object parsed from the input string, or Guid.Empty if parsing fails.</returns>
    public static Guid ToGuid(this string? str)
    {
        return Guid.TryParse(str, out var result) ? result : Guid.Empty;
    }
}