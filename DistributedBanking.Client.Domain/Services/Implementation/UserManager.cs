using Contracts.Extensions;
using Contracts.Models;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Models.Identity;
using Mapster;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class UserManager : IUserManager
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IPasswordHashingService _passwordService;
    private readonly ILogger<UserManager> _logger;

    public UserManager(
        IUsersRepository usersRepository,
        IRolesRepository rolesManager, 
        IPasswordHashingService passwordService,
        ILogger<UserManager> logger)
    {
        _usersRepository = usersRepository;
        _rolesRepository = rolesManager;
        _passwordService = passwordService;
        _logger = logger;
    }
    
    public async Task<UserModel?> FindByEmailAsync(string email)
    {
        try
        {
            var user = await _usersRepository.GetByEmailAsync(email);

            return user?.Adapt<UserModel>();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred while trying to find user by email");

            return null;
        }
    }

    public async Task<UserModel?> FindByIdAsync(string id)
    {
        try
        {
            var user = await _usersRepository.GetAsync(new ObjectId(id));

            return user?.Adapt<UserModel>();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred while trying to find user by email");

            return null;
        }
    }

    public async Task<IdentityOperationResult> PasswordSignInAsync(string email, string password)
    {
        try
        {
            var user = await _usersRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return IdentityOperationResult.Failed("User with the specified email doesn't exist");
            }
            
            var successfulSignIn = _passwordService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);

            return successfulSignIn
                ? IdentityOperationResult.Success
                : IdentityOperationResult.Failed("Incorrect email or password");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred while trying to sign in user");
            return IdentityOperationResult.Failed();
        }
    }

    public async Task<IEnumerable<string>> GetRolesAsync(ObjectId userId)
    {
        var user = await _usersRepository.GetAsync(userId);
        if (user == null)
        {
            _logger.LogError("User with the specified ID '{Id}' does not exist", userId.ToString());
            return Array.Empty<string>();
        }
        
        if (!user.Roles.Any())
        {
            return Array.Empty<string>();
        }

        var roleNames = new List<string>();
        foreach (var roleId in user.Roles)
        {
            var role = await _rolesRepository.GetAsync(new ObjectId(roleId));
            if (role == null)
            {
                _logger.LogError("Role with the ID {RoleId} that was specified for user {UserId}", roleId, userId.ToString());
            }
            else
            {
                roleNames.Add(role.Name);
            }
        }

        return roleNames;
    }

    public async Task<bool> IsInRoleAsync(ObjectId userId, string roleName)
    {
        var user = await _usersRepository.GetAsync(userId);
        if (user == null)
        {
            _logger.LogError("User with the specified ID '{Id}' does not exist", userId.ToString());
            return false;
        }
        
        var roleId = (await _rolesRepository.GetAsync(r => r.NormalizedName == roleName.NormalizeString())).FirstOrDefault()?.Id;
        
        return roleId != null && user.Roles.Contains(roleId.Value.ToString());
    }
}