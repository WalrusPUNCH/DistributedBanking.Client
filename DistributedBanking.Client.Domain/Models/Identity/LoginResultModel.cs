namespace DistributedBanking.Client.Domain.Models.Identity;

public class LoginResultModel
{
    public string Token { get; set; }
    public bool IsAdmin { get; set; }
}