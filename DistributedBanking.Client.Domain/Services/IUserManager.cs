using Contracts.Models;
using DistributedBanking.Client.Domain.Models.Identity;
using MongoDB.Bson;

namespace DistributedBanking.Client.Domain.Services;

public interface IUserManager
{
    Task<UserModel?> FindByEmailAsync(string email);
    Task<UserModel?> FindByIdAsync(string id);
    Task<IdentityOperationResult> PasswordSignInAsync(string email, string password);
    Task<IEnumerable<string>> GetRolesAsync(ObjectId userId);
    Task<bool> IsInRoleAsync(ObjectId userId, string roleName);
}
