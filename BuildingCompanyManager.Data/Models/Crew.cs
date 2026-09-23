using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingCompanyManager.Data.Common;

namespace BuildingCompanyManager.Data.Models;

public class Crew
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    [Display(Name = EntityDisplayNames.TechnicalManager)]
    public int TechnicalManagerId { get; set; }

    [Required]
    [Display(Name = EntityDisplayNames.Foreman)]
    public int ForemanId { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.CompanyNameMaxLength)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(TechnicalManagerId))]
    public Employee TechnicalManager { get; set; } = null!;

    [ForeignKey(nameof(ForemanId))]
    public Employee Foreman { get; set; } = null!;

    public ICollection<Employee> Members { get; set; } = new HashSet<Employee>();

    public ICollection<ProjectCrew> ProjectCrews { get; set; } = new HashSet<ProjectCrew>();
}
