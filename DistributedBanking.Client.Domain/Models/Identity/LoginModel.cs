namespace DistributedBanking.Client.Domain.Models.Identity;

public class LoginModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}