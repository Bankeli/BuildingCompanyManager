using System.ComponentModel.DataAnnotations;

namespace BuildingCompanyManager.Models;

public class CompanyRegistrationInputModel
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        @"^(\d{9}|\d{13})$",
        ErrorMessage = "EIK must contain exactly 9 or 13 digits.")]
    [Display(Name = "EIK")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "First name")]
    public string OwnerFirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Last name")]
    public string OwnerLastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "{0} must be at least {2} characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
