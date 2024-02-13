namespace DistributedBanking.Client.Domain.Services;

public interface IRolesManager
{
    Task<bool> RoleExists(string roleName);
}