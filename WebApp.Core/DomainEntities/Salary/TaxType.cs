namespace WebApp.Core.DomainEntities.Salary;

public enum TaxType
{
    NonResident = 1, // Không cư trú 20%
    ResidentNonContract = 2, // Cư trú cố định 10%
    ResidentProgressive = 3, // Cư trú lũy tiến
}