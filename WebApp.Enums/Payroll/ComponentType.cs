namespace WebApp.Enums.Payroll;

public struct ComponentType
{
    public const string Fixed = "FIXED";
    public const string Input = "INPUT";
    public const string Calc = "CALC";
    
}

public struct ComponentTypeCode
{
    public const string BaseSalary = "BASE_SALARY";
    public const string WorkingSalary = "WORKING_SALARY";
    public const string ActualWorkingDays = "ACTUAL_WORKING_DAYS";
    public const string MealAllowance = "MEAL_ALLOWANCE";
    public const string TravelAllowance = "TRAVEL_ALLOWANCE";
    public const string BusinessTripAllowancePerDay = "BUSINESS_TRIP_ALLOWANCE_PER_DAY";
    public const string BusinessTripDays = "TRIP_DAYS";
    public const string BusinessTripAllowance = "BUSINESS_TRIP_ALLOWANCE";
}