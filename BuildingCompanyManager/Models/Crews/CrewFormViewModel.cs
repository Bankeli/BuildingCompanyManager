using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Data.Common;
using BuildingCompanyManager.Models.Shared;

namespace BuildingCompanyManager.Models.Crews;

public class CrewFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.CompanyNameMaxLength)]
    [Display(Name = "Crew name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Technical manager")]
    public int? TechnicalManagerId { get; set; }

    [Required]
    [Display(Name = "Foreman")]
    public int? ForemanId { get; set; }

    [Display(Name = "Workers")]
    public List<int> WorkerIds { get; set; } = [];

    public IReadOnlyCollection<EmployeeOptionViewModel> TechnicalManagers { get; set; } = [];

    public IReadOnlyCollection<EmployeeOptionViewModel> Foremen { get; set; } = [];

    public IReadOnlyCollection<EmployeeOptionViewModel> Workers { get; set; } = [];
}
