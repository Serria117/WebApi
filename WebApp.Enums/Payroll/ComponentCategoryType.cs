namespace WebApp.Enums.Payroll;

public struct ComponentCategoryType
{
    public const string TaxableIncome = "Thu nhập chịu thuế";
    public const string NonTaxableIncome = "Thu nhập không chịu thuế";
    public const string Deduction = "Khấu trừ";
    public const string PersonalIncomeTax = "Thuế TNCN";
    
    /// <summary>
    /// Get all fields of the ComponentCategoryType struct.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string?> GetFields()
    {
        return typeof(ComponentCategoryType)
               .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
               .Select(f => f.GetValue(null)?.ToString());
    }
}