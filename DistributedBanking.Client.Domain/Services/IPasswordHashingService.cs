namespace DistributedBanking.Client.Domain.Services;

public interface IPasswordHashingService
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
}