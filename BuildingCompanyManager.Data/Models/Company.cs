using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Data.Common;

namespace BuildingCompanyManager.Data.Models;

public class Company
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.CompanyNameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityValidationConstants.RegistrationNumberMaxLength)]
    [Display(Name = EntityDisplayNames.RegistrationNumber)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityValidationConstants.AddressMaxLength)]
    public string Address { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();

    public ICollection<Crew> Crews { get; set; } = new HashSet<Crew>();

    public ICollection<Project> Projects { get; set; } = new HashSet<Project>();
}
