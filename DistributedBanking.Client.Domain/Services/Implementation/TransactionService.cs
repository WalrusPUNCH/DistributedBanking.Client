using Confluent.Kafka;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Mapping;
using DistributedBanking.Client.Domain.Models.Transaction;
using Mapster;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Kafka.Messages.Transaction;
using Shared.Kafka.Services;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class TransactionService : ITransactionService
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly ILogger<TransactionService> _logger;
    private readonly IKafkaProducerService<TransactionMessage> _transactionsProducer;

    public TransactionService(
        ITransactionsRepository transactionsRepository, 
        IAccountsRepository accountsRepository,
        ILogger<TransactionService> logger, 
        IKafkaProducerService<TransactionMessage> transactionsProducer)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
        _logger = logger;
        _transactionsProducer = transactionsProducer;
    }
    
    public async Task<bool> Deposit(OneWayTransactionModel depositTransactionModel)
    {
        var account = await _accountsRepository.GetAsync(new ObjectId(depositTransactionModel.SourceAccountId));
        if (account == null || !AccountValidator.IsAccountValid(account))
        {
            return false;
        }
        
        var transactionMessage = depositTransactionModel.ToKafkaMessage();
        var result = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
        
        return result.Status == PersistenceStatus.Persisted;
    }
    
    public async Task<bool> Withdraw(OneWaySecuredTransactionModel withdrawTransactionModel)
    {
        var account = await _accountsRepository.GetAsync(new ObjectId(withdrawTransactionModel.SourceAccountId));
        if (account == null || !AccountValidator.IsAccountValid(account, withdrawTransactionModel.SecurityCode))
        {
            return false;
        }
        
        var transactionMessage = withdrawTransactionModel.ToKafkaMessage();
        var result = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
        
        return result.Status == PersistenceStatus.Persisted;
    }
    
    public async Task<bool> Transfer(TwoWayTransactionModel transferTransactionModel)
    {
        var destinationAccount = await _accountsRepository.GetAsync(new ObjectId(transferTransactionModel.DestinationAccountId));
        var sourceAccount = await _accountsRepository.GetAsync(new ObjectId(transferTransactionModel.SourceAccountId));
        if (sourceAccount == null || !AccountValidator.IsAccountValid(sourceAccount, transferTransactionModel.SourceAccountSecurityCode))
        {
            return false;
        }
            
        if (destinationAccount == null || !AccountValidator.IsAccountValid(destinationAccount))
        {
            return false;
        }

        var transactionMessage = transferTransactionModel.ToKafkaMessage();
        var result = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
            
        return result.Status == PersistenceStatus.Persisted;
    }
    
    public async Task<decimal> GetBalance(string accountId)
    {
        var account = await _accountsRepository.GetAsync(new ObjectId(accountId));

        return account?.Balance ?? default;
    }

    public async Task<IEnumerable<TransactionResponseModel>> GetAccountTransactionHistory(string accountId)
    {
        var transactions = await _transactionsRepository.AccountTransactionHistory(accountId);

        return transactions.Adapt<TransactionResponseModel[]>();
    }
}