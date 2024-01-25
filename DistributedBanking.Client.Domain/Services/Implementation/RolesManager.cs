using Contracts.Extensions;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Models.Identity;
using Microsoft.Extensions.Logging;
using Shared.Data.Entities.Identity;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class RolesManager : IRolesManager
{
    private readonly IRolesRepository _rolesRepository;
    private readonly ILogger<RolesManager> _logger;

    public RolesManager(
        IRolesRepository rolesRepository,
        ILogger<RolesManager> logger)
    {
        _rolesRepository = rolesRepository;
        _logger = logger;
    }
    
    public async Task<IdentityOperationResult> CreateAsync(ApplicationRole role)
    {
        try
        {
            await _rolesRepository.AddAsync(role);
            return IdentityOperationResult.Success;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred while trying to create new role");
            
            return IdentityOperationResult.Failed();
        }
    }

    public async Task<bool> RoleExists(string roleName)
    {
        return (await _rolesRepository.GetAsync(r => r.NormalizedName == roleName.NormalizeString())).Any();
    }
}