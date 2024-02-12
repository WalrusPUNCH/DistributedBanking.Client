using Confluent.Kafka;
using Contracts.Models;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Mapping;
using DistributedBanking.Client.Domain.Models.Account;
using Mapster;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Data.Entities;
using Shared.Kafka.Services;
using Shared.Messaging.Messages.Account;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class AccountService : IAccountService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IKafkaProducerService<AccountCreationMessage> _accountCreationProducer;
    private readonly IKafkaProducerService<AccountDeletionMessage> _accountDeletionProducer;
    private readonly IResponseService _responseService;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountsRepository accountsRepository, 
        IKafkaProducerService<AccountCreationMessage> accountCreationProducer, 
        IKafkaProducerService<AccountDeletionMessage> accountDeletionProducer,
        IResponseService responseService,
        ILogger<AccountService> logger)
    {
        _accountsRepository = accountsRepository;
        _accountCreationProducer = accountCreationProducer;
        _accountDeletionProducer = accountDeletionProducer;
        _responseService = responseService;
        _logger = logger;
    }
    
    public async Task<OperationStatusModel<AccountOwnedResponseModel>> CreateAsync(string customerId, AccountCreationModel accountCreationModel)
    {
        var accountCreationMessage = accountCreationModel.ToKafkaMessage(customerId);
        
        var messageDelivery = await _accountCreationProducer.ProduceAsync(accountCreationMessage, accountCreationMessage.Headers);
        if (messageDelivery.Status != PersistenceStatus.Persisted)
        {
            _logger.LogError("Unable to publish customer personal information update message into Kafka. Message was not persisted");
            return OperationStatusModel<AccountOwnedResponseModel>.Fail("Error occurred while trying to update personal information");
        }
        
        var response = await _responseService.GetResponse<OperationStatusModel<AccountOwnedResponseModel>>(accountCreationMessage, messageDelivery.TopicPartitionOffset);
        return response ?? OperationStatusModel<AccountOwnedResponseModel>.Processing();
    }

    public async Task<AccountOwnedResponseModel?> GetAsync(string id)
    {
        var account = await _accountsRepository.GetAsync(new ObjectId(id));

        return account.Adapt<AccountOwnedResponseModel>();
    }
    
    public async Task<IEnumerable<AccountOwnedResponseModel>> GetAsync()
    {
        var accounts = await _accountsRepository.GetAsync();
        
        return accounts.Adapt<AccountOwnedResponseModel[]>();
    }

    public async Task<IEnumerable<AccountResponseModel>> GetCustomerAccountsAsync(string customerId)
    {
        var accounts = await _accountsRepository.GetAsync(x => x.Owner == customerId);
        
        return accounts.Adapt<AccountResponseModel[]>();
    }

    public async Task<bool> BelongsTo(string accountId, string customerId)
    {
        var account = await _accountsRepository.GetAsync(
            a => a.Id == new ObjectId(accountId) && a.Owner != null && a.Owner == customerId);

        return account.Any();
    }
    
    public async Task UpdateAsync(AccountEntity model)
    {
        await _accountsRepository.UpdateAsync(model);
    }

    public async Task<OperationStatusModel> DeleteAsync(string id)
    {
        var accountEntity = await _accountsRepository.GetAsync(new ObjectId(id));
        if (string.IsNullOrWhiteSpace(accountEntity?.Owner))
        {
            _logger.LogWarning("Unable to delete account '{AccountId}' because such account does not exist or already deleted", id);
            return OperationStatusModel.Fail("Error occured while trying to delete account. Try again later");
        }

        var accountDeletionMessage = new AccountDeletionMessage(id);
        var messageDelivery = await _accountDeletionProducer.ProduceAsync(accountDeletionMessage, accountDeletionMessage.Headers);
        if (messageDelivery.Status != PersistenceStatus.Persisted)
        {
            _logger.LogError("Unable to publish account deletion message into Kafka. Message was not persisted");
            return OperationStatusModel<AccountOwnedResponseModel>.Fail("Error occurred while trying to delete account");
        }
        
        var response = await _responseService.GetResponse<OperationStatusModel>(accountDeletionMessage, messageDelivery.TopicPartitionOffset);
        return response ?? OperationStatusModel.Processing();
    }
}