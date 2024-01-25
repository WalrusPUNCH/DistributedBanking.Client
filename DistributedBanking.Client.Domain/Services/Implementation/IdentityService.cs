using Contracts;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Models.Identity;
using Mapster;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Data.Entities.Constants;
using Shared.Data.Entities.EndUsers;
using Shared.Data.Entities.Identity;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class IdentityService : IIdentityService
{
    private readonly IUserManager _usersManager;
    private readonly IRolesManager _rolesManager;
    private readonly ITokenService _tokenService;
    private readonly ICustomersRepository _customersRepository;
    private readonly IWorkersRepository _workersRepository;
    private readonly IAccountService _accountService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        IUserManager userManager,
        IRolesManager rolesManager,
        ITokenService tokenService,
        ICustomersRepository customersRepository,
        IWorkersRepository workersRepository,
        IAccountService accountService,
        ILogger<IdentityService> logger)
    {
        _usersManager = userManager;
        _rolesManager = rolesManager;
        _tokenService = tokenService;
        _customersRepository = customersRepository;
        _workersRepository = workersRepository;
        _accountService = accountService;
        _logger = logger;
    }

    public async Task<IdentityOperationResult> CreateRole(string roleName)
    {
        if (await _rolesManager.RoleExists(roleName))
        {
            return IdentityOperationResult.Failed("Role with this name already exists");
        }
        
        var result = await _rolesManager.CreateAsync(new ApplicationRole(roleName));
        if (result.Succeeded)
        {
            _logger.LogInformation("New role '{Role}' has been created", roleName);
            
            return IdentityOperationResult.Success;
        }
        
        return IdentityOperationResult.Failed("Unable to create new role. Try again later");
    }

    public async Task<IdentityOperationResult> RegisterUser(
        EndUserRegistrationModel registrationModel, string role)
    {
        return await RegisterUserInternal(registrationModel, role);
    }
    
    private async Task<IdentityOperationResult> RegisterUserInternal(EndUserRegistrationModel registrationModel, string role)
    {
        var existingUser = await _usersManager.FindByEmailAsync(registrationModel.Email);
        if (existingUser != null)
        {
            return IdentityOperationResult.Failed("User with the same email already exists");
        }
        
        ObjectId endUserId;
        if (string.Equals(role, RoleNames.Customer, StringComparison.InvariantCultureIgnoreCase))
        {
            var customerEntity = registrationModel.Adapt<CustomerEntity>();
            await _customersRepository.AddAsync(customerEntity);

            endUserId = customerEntity.Id;
        }
        else if (string.Equals(role, RoleNames.Worker, StringComparison.InvariantCultureIgnoreCase))
        {
            var workerEntity = registrationModel.Adapt<WorkerEntity>();
            await _workersRepository.AddAsync(workerEntity);
            
            endUserId = workerEntity.Id;
        }
        else if (string.Equals(role, RoleNames.Administrator, StringComparison.InvariantCultureIgnoreCase))
        {
            var workerEntity = registrationModel.Adapt<WorkerEntity>();
            await _workersRepository.AddAsync(workerEntity);
            
            endUserId = workerEntity.Id;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Specified role is not supported");
        }
        
        var userCreationResult = await _usersManager.CreateAsync(endUserId.ToString()!, registrationModel, new []{ role });
        if (!userCreationResult.Succeeded)
        {
            return userCreationResult;
        }
        
        _logger.LogInformation("New user '{Email}' has been registered and assigned a '{Role}' role",
            registrationModel.Email, role);
            
        return userCreationResult;
    }

    public async Task<(IdentityOperationResult LoginResult, string? Token)> Login(LoginModel loginModel)
    {
        var appUser = await _usersManager.FindByEmailAsync(loginModel.Email);
        if (appUser == null)
        {
            return (IdentityOperationResult.Failed("User with such email doesn't exist"), default);
        }
        
        var loginResult = await _usersManager.PasswordSignInAsync(loginModel.Email, loginModel.Password);
        if (!loginResult.Succeeded)
        {
            return (IdentityOperationResult.Failed("Incorrect email or password"), default);
        }
        
        var token = await _tokenService.GenerateTokenAsync(appUser);
        return (loginResult, token);
    }

    public async Task Logout()
    {
        
    }

    public async Task DeleteUser(string email)
    {
        var appUser = await _usersManager.FindByEmailAsync(email);
        if (appUser != null)
        {
            if (await _usersManager.IsInRoleAsync(appUser.Id, RoleNames.Customer))
            {
                var customer = await _customersRepository.GetAsync(new ObjectId(appUser.EndUserId));
                foreach (var customerAccountId in customer.Accounts)
                {
                    await _accountService.DeleteAsync(customerAccountId);
                }
                
                await _customersRepository.RemoveAsync(new ObjectId(appUser.EndUserId));
            }
            else if (await _usersManager.IsInRoleAsync(appUser.Id, RoleNames.Worker))
            {
                await _workersRepository.RemoveAsync(new ObjectId(appUser.EndUserId));
            }
            
            await _usersManager.DeleteAsync(appUser.Id);
        }
    }

    public async Task<OperationStatusModel> UpdateCustomerPersonalInformation(string customerId, CustomerPassportModel customerPassport)
    {
        try
        {
            var customer = await _customersRepository.GetAsync(new ObjectId(customerId));
            customer.Passport = customerPassport.Adapt<CustomerPassport>();

            await _customersRepository.UpdateAsync(customer);
            
            return OperationStatusModel.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to update personal information. Try again later");
            throw;
        }
    }
}