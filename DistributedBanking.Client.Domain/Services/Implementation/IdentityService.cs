using Confluent.Kafka;
using Contracts.Models;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Mapping;
using DistributedBanking.Client.Domain.Models.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Data.Entities.Identity;
using Shared.Kafka.Services;
using Shared.Messaging.Messages.Identity;
using Shared.Messaging.Messages.Identity.Registration;

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
    private readonly IResponseService _responseService;

    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        IUserManager userManager,
        IRolesManager rolesManager,
        ITokenService tokenService,
        IPasswordHashingService passwordHashingService,
        ICustomersRepository customersRepository,
        IKafkaProducerService<UserRegistrationMessage> userRegistrationProducer, 
        IKafkaProducerService<WorkerRegistrationMessage> workerRegistrationProducer, 
        IKafkaProducerService<EndUserDeletionMessage> endUserDeletionProducer, 
        IKafkaProducerService<CustomerInformationUpdateMessage> customerInformationUpdateProducer,
        IResponseService responseService,
        ILogger<IdentityService> logger)
    {
        _usersManager = userManager;
        _rolesManager = rolesManager;
        _tokenService = tokenService;
        _passwordHashingService = passwordHashingService;
        _customersRepository = customersRepository;
        _userRegistrationProducer = userRegistrationProducer;
        _workerRegistrationProducer = workerRegistrationProducer;
        _endUserDeletionProducer = endUserDeletionProducer;
        _customerInformationUpdateProducer = customerInformationUpdateProducer;
        _responseService = responseService;
        _logger = logger;
    }

    public async Task<IdentityOperationResult> CreateRole(string roleName)
    {
        if (await _rolesManager.RoleExists(roleName))
        {
            return IdentityOperationResult.Failed("Role with the specified name already exists");
        }
        
        var result = await _rolesManager.CreateAsync(new ApplicationRole(roleName));
        if (result.Succeeded)
        {
            _logger.LogInformation("New role '{Role}' has been created", roleName);
            
            return IdentityOperationResult.Success;
        }
        
        return IdentityOperationResult.Failed("Unable to create new role. Try again later");
    }

    public async Task<IdentityOperationResult> RegisterCustomer(EndUserRegistrationModel registrationModel)
    {
        var existingUser = await _usersManager.FindByEmailAsync(registrationModel.Email);
        if (existingUser != null)
        {
            return IdentityOperationResult.Failed("A user with the same email is already registered");
        }
        
        var passwordHash = _passwordHashingService.HashPassword(registrationModel.Password, out var salt);
        var userRegistrationMessage = registrationModel.ToKafkaMessage(passwordHash, salt);
        var messageDelivery = await _userRegistrationProducer.ProduceAsync(userRegistrationMessage, userRegistrationMessage.Headers);
        if (messageDelivery.Status != PersistenceStatus.Persisted)
        {
            _logger.LogError("Unable to publish customer registration message into Kafka. Message was not persisted");
            return IdentityOperationResult.Failed("Error occurred while trying to register new user");
        }
        
        var response = await _responseService.GetResponse<IdentityOperationResult>(userRegistrationMessage, messageDelivery.TopicPartitionOffset);
        
        return response ?? IdentityOperationResult.Processing;
    }
    
    public async Task<IdentityOperationResult> RegisterWorker(WorkerRegistrationModel registrationModel, string role)
    {
        var existingWorker = await _usersManager.FindByEmailAsync(registrationModel.Email);
        if (existingWorker != null)
        {
            return IdentityOperationResult.Failed("A user with the same email is already registered");
        }
        
        var passwordHash = _passwordHashingService.HashPassword(registrationModel.Password, out var salt);
        var workerRegistrationMessage = registrationModel.ToKafkaMessage(role, passwordHash, salt);
        var messageDelivery = await _workerRegistrationProducer.ProduceAsync(workerRegistrationMessage, workerRegistrationMessage.Headers);
        if (messageDelivery.Status != PersistenceStatus.Persisted)
        {
            _logger.LogError("Unable to publish worker registration message into Kafka. Message was not persisted");
            return IdentityOperationResult.Failed("Error occurred while trying to register new user");
        }
        
        var response = await _responseService.GetResponse<IdentityOperationResult>(workerRegistrationMessage, messageDelivery.TopicPartitionOffset);
        
        return response ?? IdentityOperationResult.Processing;
    }
    
    public async Task<IdentityOperationResult> UpdateCustomerPersonalInformation(string customerId, CustomerPassportModel customerPassport)
    {
        var customer = await _customersRepository.GetAsync(new ObjectId(customerId));
        if (customer == null)
        {
            return IdentityOperationResult.Failed("User with the specified email does not exist");
        }

        var customerInformationUpdateMessage = customerPassport.ToKafkaMessage(customerId);
        var messageDelivery = await _customerInformationUpdateProducer.ProduceAsync(
            value: customerInformationUpdateMessage,
            headers: customerInformationUpdateMessage.Headers);
        
        if (messageDelivery.Status != PersistenceStatus.Persisted)
        {
            _logger.LogError("Unable to publish customer personal information update message into Kafka. Message was not persisted");
            return IdentityOperationResult.Failed("Error occurred while trying to update personal information");
        }
        
        var response = await _responseService.GetResponse<IdentityOperationResult>(customerInformationUpdateMessage, messageDelivery.TopicPartitionOffset);
        return response ?? IdentityOperationResult.Processing;
    }
    
    public async Task<IdentityOperationResult> DeleteUser(string email)
    {
        var appUser = await _usersManager.FindByEmailAsync(email);
        if (appUser == null)
        {
            return IdentityOperationResult.Failed("User with the specified email does not exist");
        }

        var endUserDeletionMessage = new EndUserDeletionMessage(appUser.EndUserId);
        var messageDelivery = await _endUserDeletionProducer.ProduceAsync(endUserDeletionMessage, endUserDeletionMessage.Headers);
        if (messageDelivery.Status != PersistenceStatus.Persisted)
        {
            _logger.LogError("Unable to publish user deletion message into Kafka. Message was not persisted");
            return IdentityOperationResult.Failed("Error occurred while trying to delete user");
        }
        
        var response = await _responseService.GetResponse<IdentityOperationResult>(endUserDeletionMessage, messageDelivery.TopicPartitionOffset);
        
        return response ?? IdentityOperationResult.Processing;
    }
    
    public async Task<(IdentityOperationResult LoginResult, string? Token)> Login(LoginModel loginModel)
    {
        var appUser = await _usersManager.FindByEmailAsync(loginModel.Email);
        if (appUser == null)
        {
            return (IdentityOperationResult.Failed("User with the specified email does not exist"), default);
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
}