using Confluent.Kafka;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Mapping;
using DistributedBanking.Client.Domain.Models.Account;
using Mapster;
using MongoDB.Bson;
using Shared.Data.Entities;
using Shared.Kafka.Messages.Account;
using Shared.Kafka.Services;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class AccountService : IAccountService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IKafkaProducerService<AccountCreationMessage> _accountCreationProducer;
    private readonly IKafkaProducerService<AccountDeletionMessage> _accountDeletionProducer;

    public AccountService(
        IAccountsRepository accountsRepository, 
        IKafkaProducerService<AccountCreationMessage> accountCreationProducer, 
        IKafkaProducerService<AccountDeletionMessage> accountDeletionProducer)
    {
        _accountsRepository = accountsRepository;
        _accountCreationProducer = accountCreationProducer;
        _accountDeletionProducer = accountDeletionProducer;
    }
    
    public async Task<bool> CreateAsync(string customerId, AccountCreationModel accountCreationModel)
    {
        var accountCreationMessage = accountCreationModel.ToKafkaMessage(customerId);
        
        var result = await _accountCreationProducer.ProduceAsync(accountCreationMessage, accountCreationMessage.Headers);
        return result.Status == PersistenceStatus.Persisted;
    }

    public async Task<AccountOwnedResponseModel> GetAsync(string id)
    {
        var account = await _accountsRepository.GetAsync(new ObjectId(id));

        return account.Adapt<AccountOwnedResponseModel>();
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

    public async Task<IEnumerable<AccountOwnedResponseModel>> GetAsync()
    {
        var accounts = await _accountsRepository.GetAsync();
        
        return accounts.Adapt<AccountOwnedResponseModel[]>();
    }

    public async Task UpdateAsync(AccountEntity model)
    {
        await _accountsRepository.UpdateAsync(model);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var accountEntity = await _accountsRepository.GetAsync(new ObjectId(id));
        if (string.IsNullOrWhiteSpace(accountEntity?.Owner))
        {
            return false;
        }
        
        var accountDeletionMessage = new AccountDeletionMessage(id);
        var result = await _accountDeletionProducer.ProduceAsync(accountDeletionMessage, accountDeletionMessage.Headers);
        
        return result.Status == PersistenceStatus.Persisted;
    }
}