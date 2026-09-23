using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingCompanyManager.Data.Common;
using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Data.Models;

public class Project
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    [Display(Name = EntityDisplayNames.TechnicalManager)]
    public int TechnicalManagerId { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.ProjectNameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityValidationConstants.CompanyNameMaxLength)]
    [Display(Name = EntityDisplayNames.ClientName)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityValidationConstants.AddressMaxLength)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = EntityDisplayNames.StartDate)]
    public DateOnly StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = EntityDisplayNames.EndDate)]
    public DateOnly? EndDate { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(TechnicalManagerId))]
    public Employee TechnicalManager { get; set; } = null!;

    public ICollection<ProjectCrew> ProjectCrews { get; set; } = new HashSet<ProjectCrew>();
}
