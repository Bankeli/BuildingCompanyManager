using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Data.Common;
using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Models.Employees;

public class EmployeeEditInputModel
{
    public int Id { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public EmployeeRole? Role { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.JobTitleMaxLength)]
    [Display(Name = "Job title")]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), EntityValidationConstants.MinimumPositiveAmount, EntityValidationConstants.MaximumMoneyAmount)]
    [Display(Name = "Daily rate")]
    public decimal? DailyRate { get; set; }
}
