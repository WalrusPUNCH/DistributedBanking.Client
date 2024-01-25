using System.ComponentModel.DataAnnotations;

namespace DistributedBanking.API.Models.Identity;

public class EndUserRegistrationDto
{
    [Required]
    public required string FirstName { get; set; }
    
    [Required]
    public required string LastName { get; set; }
    
    [Required]
    public required DateTime BirthDate { get; set; }

    [Required, Phone]
    public required string PhoneNumber { get; set; }
 
    [Required, EmailAddress(ErrorMessage = "Invalid Email")]
    public required string Email { get; set; }
 
    [Required]
    public required string Password { get; set; }
    
    [Required]
    public required CustomerPassportDto Passport { get; set; }
}