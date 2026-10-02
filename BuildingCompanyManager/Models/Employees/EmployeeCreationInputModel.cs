using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Data.Common;
using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Models.Employees;

public class EmployeeCreationInputModel
{
    [Required]
    [StringLength(EntityValidationConstants.PersonNameMaxLength)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityValidationConstants.PersonNameMaxLength)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(EntityValidationConstants.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

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
