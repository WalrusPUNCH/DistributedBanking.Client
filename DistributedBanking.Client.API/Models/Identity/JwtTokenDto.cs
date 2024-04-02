namespace DistributedBanking.API.Models.Identity;

public class JwtTokenDto
{
    public required string Token { get; set; }
    public required bool IsAdmin { get; set; }
}