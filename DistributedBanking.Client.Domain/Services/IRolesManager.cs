using DistributedBanking.Client.Domain.Models.Identity;
using Shared.Data.Entities.Identity;

namespace DistributedBanking.Client.Domain.Services;

public interface IRolesManager
{
    Task<IdentityOperationResult> CreateAsync(ApplicationRole role);
    Task<bool> RoleExists(string roleName);
}