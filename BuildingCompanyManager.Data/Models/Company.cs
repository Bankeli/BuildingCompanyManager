using System.ComponentModel.DataAnnotations;

namespace BuildingCompanyManager.Data.Models;

public class Company
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 9)]
    [Display(Name = "Registration number")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();

    public ICollection<Crew> Crews { get; set; } = new HashSet<Crew>();

    public ICollection<Project> Projects { get; set; } = new HashSet<Project>();
}
