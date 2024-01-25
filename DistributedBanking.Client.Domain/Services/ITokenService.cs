using DistributedBanking.Client.Domain.Models.Identity;

namespace DistributedBanking.Client.Domain.Services;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(UserModel user);
}