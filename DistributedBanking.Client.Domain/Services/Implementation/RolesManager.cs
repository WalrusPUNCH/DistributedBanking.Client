using Contracts.Extensions;
using DistributedBanking.Client.Data.Repositories;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class RolesManager : IRolesManager
{
    private readonly IRolesRepository _rolesRepository;

    public RolesManager(IRolesRepository rolesRepository)
    {
        _rolesRepository = rolesRepository;
    }
    
    public async Task<bool> RoleExists(string roleName)
    {
        return (await _rolesRepository.GetAsync(r => r.NormalizedName == roleName.NormalizeString())).Any();
    }
}