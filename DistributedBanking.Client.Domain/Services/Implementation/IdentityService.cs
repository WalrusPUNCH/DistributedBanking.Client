using Confluent.Kafka;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Mapping;
using DistributedBanking.Client.Domain.Models.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Data.Entities.Identity;
using Shared.Kafka.Messages.Identity;
using Shared.Kafka.Messages.Identity.Registration;
using Shared.Kafka.Services;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class IdentityService : IIdentityService
{
    private readonly IUserManager _usersManager;
    private readonly IRolesManager _rolesManager;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ICustomersRepository _customersRepository;
    private readonly IKafkaProducerService<UserRegistrationMessage> _userRegistrationProducer;
    private readonly IKafkaProducerService<WorkerRegistrationMessage> _workerRegistrationProducer;
    private readonly IKafkaProducerService<EndUserDeletionMessage> _endUserDeletionProducer;
    private readonly IKafkaProducerService<CustomerInformationUpdateMessage> _customerInformationUpdateProducer;
        
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        IUserManager userManager,
        IRolesManager rolesManager,
        ITokenService tokenService,
        IPasswordHashingService passwordHashingService,
        ICustomersRepository customersRepository,
        ILogger<IdentityService> logger, 
        IKafkaProducerService<UserRegistrationMessage> userRegistrationProducer, 
        IKafkaProducerService<WorkerRegistrationMessage> workerRegistrationProducer, 
        IKafkaProducerService<EndUserDeletionMessage> endUserDeletionProducer, 
        IKafkaProducerService<CustomerInformationUpdateMessage> userInformationUpdateProducer)
    {
        _usersManager = userManager;
        _rolesManager = rolesManager;
        _tokenService = tokenService;
        _passwordHashingService = passwordHashingService;
        _customersRepository = customersRepository;
        _logger = logger;
        _userRegistrationProducer = userRegistrationProducer;
        _workerRegistrationProducer = workerRegistrationProducer;
        _endUserDeletionProducer = endUserDeletionProducer;
        _customerInformationUpdateProducer = userInformationUpdateProducer;
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

    public async Task<bool> RegisterCustomer(EndUserRegistrationModel registrationModel)
    {
        var existingUser = await _usersManager.FindByEmailAsync(registrationModel.Email);
        if (existingUser != null)
        {
            return false;
        }
        
        var passwordHash = _passwordHashingService.HashPassword(registrationModel.Password, out var salt);
        var userRegistrationMessage = registrationModel.ToKafkaMessage(passwordHash, salt);
        var result = await _userRegistrationProducer.ProduceAsync(userRegistrationMessage, userRegistrationMessage.Headers);
            
        return result.Status == PersistenceStatus.Persisted;
    }
    
    public async Task<bool> RegisterWorker(WorkerRegistrationModel registrationModel, string role)
    {
        var existingWorker = await _usersManager.FindByEmailAsync(registrationModel.Email);
        if (existingWorker != null)
        {
            return false;
        }
        
        var passwordHash = _passwordHashingService.HashPassword(registrationModel.Password, out var salt);
        var workerRegistrationMessage = registrationModel.ToKafkaMessage(role, passwordHash, salt);
        var result = await _workerRegistrationProducer.ProduceAsync(workerRegistrationMessage,workerRegistrationMessage.Headers);
        
        return result.Status == PersistenceStatus.Persisted;
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

    public async Task<bool> DeleteUser(string email)
    {
        var appUser = await _usersManager.FindByEmailAsync(email);
        if (appUser == null)
        {
            return false;
            //return IdentityOperationResult.Failed("User with such ID doesn't exist");
        }

        var endUserDeletionMessage = new EndUserDeletionMessage(appUser.EndUserId);
        var result = await _endUserDeletionProducer.ProduceAsync(endUserDeletionMessage, endUserDeletionMessage.Headers);
        
        return result.Status == PersistenceStatus.Persisted;
    }

    public async Task<bool> UpdateCustomerPersonalInformation(string customerId, CustomerPassportModel customerPassport)
    {
        var customer = await _customersRepository.GetAsync(new ObjectId(customerId));
        if (customer == null)
        {
            return false;
        }

        var customerInformationUpdateMessage = customerPassport.ToKafkaMessage(customerId);
        var result = await _customerInformationUpdateProducer.ProduceAsync(
                value: customerInformationUpdateMessage,
                headers: customerInformationUpdateMessage.Headers);
        
        return result.Status == PersistenceStatus.Persisted;
    }
}