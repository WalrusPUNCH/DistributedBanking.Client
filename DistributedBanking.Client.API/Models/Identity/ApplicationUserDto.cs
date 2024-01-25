namespace DistributedBanking.API.Models.Identity;

public class ApplicationUserDto
{
    public required string Email { get; set; }
    public DateTime CreatedOn { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}