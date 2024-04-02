namespace DistributedBanking.API.Models.Identity;

public class ShortUserModelDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    
    public required CustomerPassportDto Passport { get; set; }
}