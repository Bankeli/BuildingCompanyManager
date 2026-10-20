using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingCompanyManager.Data.Common;
using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Data.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    public int ActorEmployeeId { get; set; }

    [Required]
    public AuditActionType Action { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.AuditEntityTypeMaxLength)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public int EntityId { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.AuditDescriptionMaxLength)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(ActorEmployeeId))]
    public Employee ActorEmployee { get; set; } = null!;
}
