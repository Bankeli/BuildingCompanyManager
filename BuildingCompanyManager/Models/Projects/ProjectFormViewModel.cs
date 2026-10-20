using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Data.Common;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Models.Shared;

namespace BuildingCompanyManager.Models.Projects;

public class ProjectFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.ProjectNameMaxLength)]
    [Display(Name = "Project name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityValidationConstants.CompanyNameMaxLength)]
    [Display(Name = "Client name")]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityValidationConstants.AddressMaxLength)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Technical manager")]
    public int? TechnicalManagerId { get; set; }

    [Required]
    public ProjectStatus? Status { get; set; } = ProjectStatus.Planning;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateOnly? StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateOnly? EndDate { get; set; }

    public IReadOnlyCollection<EmployeeOptionViewModel> TechnicalManagers { get; set; } = [];
}
