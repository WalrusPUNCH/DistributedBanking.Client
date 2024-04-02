namespace DistributedBanking.Client.Domain.Models.Identity;

public class ShortUserModelResponse
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    
    public required CustomerPassportModel Passport { get; set; }
}