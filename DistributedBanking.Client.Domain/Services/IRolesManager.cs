using Contracts.Models;
using Shared.Data.Entities.Identity;

namespace DistributedBanking.Client.Domain.Services;

public interface IRolesManager
{
    Task<OperationResult> CreateAsync(ApplicationRole role);
    Task<bool> RoleExists(string roleName);
}