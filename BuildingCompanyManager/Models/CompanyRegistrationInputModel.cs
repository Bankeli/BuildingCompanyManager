using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Common;

namespace BuildingCompanyManager.Models;

public class CompanyRegistrationInputModel
{
    [Required]
    [StringLength(CompanyRegistrationValidationConstants.CompanyNameMaxLength, MinimumLength = CompanyRegistrationValidationConstants.NameMinLength)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        CompanyRegistrationValidationConstants.RegistrationNumberPattern,
        ErrorMessage = CompanyRegistrationValidationConstants.RegistrationNumberError)]
    [Display(Name = "EIK")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(CompanyRegistrationValidationConstants.AddressMaxLength, MinimumLength = CompanyRegistrationValidationConstants.AddressMinLength)]
    [Display(Name = "Company address")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(CompanyRegistrationValidationConstants.NameMaxLength, MinimumLength = CompanyRegistrationValidationConstants.NameMinLength)]
    [Display(Name = "First name")]
    public string OwnerFirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(CompanyRegistrationValidationConstants.NameMaxLength, MinimumLength = CompanyRegistrationValidationConstants.NameMinLength)]
    [Display(Name = "Last name")]
    public string OwnerLastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(
        CompanyRegistrationValidationConstants.PasswordMaxLength,
        MinimumLength = CompanyRegistrationValidationConstants.PasswordMinLength,
        ErrorMessage = CompanyRegistrationValidationConstants.PasswordLengthError)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = CompanyRegistrationValidationConstants.PasswordConfirmationError)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
