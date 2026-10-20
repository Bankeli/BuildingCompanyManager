using BuildingCompanyManager.Models.Employees;

namespace BuildingCompanyManager.Models.Payroll;

public class PayrollIndexViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public int? SelectedCrewId { get; set; }

    public int? SelectedProjectId { get; set; }

    public IReadOnlyCollection<EmployeeFilterOptionViewModel> Crews { get; set; } = [];

    public IReadOnlyCollection<EmployeeFilterOptionViewModel> Projects { get; set; } = [];

    public IReadOnlyCollection<PayrollEmployeeItemViewModel> Employees { get; set; } = [];

    public decimal TotalWorkedHours => Employees.Sum(employee => employee.WorkedHours);

    public decimal TotalBonuses => Employees.Sum(employee => employee.Bonuses);

    public decimal TotalAmount => Employees.Sum(employee => employee.TotalAmount);
}

public class PayrollEmployeeItemViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public decimal WorkedHours { get; set; }

    public decimal Bonuses { get; set; }

    public decimal TotalAmount { get; set; }
}
