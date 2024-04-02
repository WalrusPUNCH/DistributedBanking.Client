using Confluent.Kafka;
using Contracts.Models;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Mapping;
using DistributedBanking.Client.Domain.Models.Transaction;
using Mapster;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Kafka.Services;
using Shared.Messaging.Messages.Transaction;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class TransactionService : ITransactionService
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IKafkaProducerService<TransactionMessage> _transactionsProducer;
    private readonly IResponseService _responseService;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionsRepository transactionsRepository, 
        IAccountsRepository accountsRepository,
        IKafkaProducerService<TransactionMessage> transactionsProducer, 
        IResponseService responseService,
        ILogger<TransactionService> logger)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
        _transactionsProducer = transactionsProducer;
        _responseService = responseService;
        _logger = logger;
    }
    
    public async Task<OperationResult> Deposit(OneWayTransactionModel depositTransactionModel)
    {
        try
        {
            if (!ObjectId.TryParse(depositTransactionModel.SourceAccountId, out var sourceId))
            {
                return OperationResult.BadRequest("Source account id has invalid format");
            }
            
            var account = await _accountsRepository.GetAsync(sourceId);
            if (account == null || !AccountValidator.IsAccountValid(account))
            {
                return OperationResult.BadRequest("Account is expired");
            }
        
            var transactionMessage = depositTransactionModel.ToKafkaMessage();
            var messageDelivery = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
            if (messageDelivery.Status != PersistenceStatus.Persisted)
            {
                _logger.LogError("Unable to publish deposit transaction message into Kafka. Message was not persisted");
                return OperationResult.InternalFail("Error occurred while trying to make deposit");
            }
        
            var response = await _responseService.GetResponse<OperationResult>(transactionMessage, messageDelivery.TopicPartitionOffset);
        
            return response ?? OperationResult.Processing();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred while trying to make deposit into '{AccountId}' account",
                depositTransactionModel.SourceAccountId);
            
            return OperationResult.InternalFail("Error occurred while trying to make deposit");
        }
    }
    
    public async Task<OperationResult> Withdraw(OneWaySecuredTransactionModel withdrawTransactionModel)
    {
        try
        {
            if (!ObjectId.TryParse(withdrawTransactionModel.SourceAccountId, out var sourceId))
            {
                return OperationResult.BadRequest("Source account id has invalid format");
            }
            
            var account = await _accountsRepository.GetAsync(sourceId);
            if (account == null || !AccountValidator.IsAccountValid(account, withdrawTransactionModel.SecurityCode))
            {
                return OperationResult.BadRequest("Provided account information is not valid. Account is expired or entered " +
                                                  "security code is not correct");
            }
        
            var transactionMessage = withdrawTransactionModel.ToKafkaMessage();
            var messageDelivery = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
            if (messageDelivery.Status != PersistenceStatus.Persisted)
            {
                _logger.LogError("Unable to publish withdrawal transaction message into Kafka. Message was not persisted");
                return OperationResult.InternalFail("Error occurred while trying to make withdrawal");
            }
        
            var response = await _responseService.GetResponse<OperationResult>(transactionMessage, messageDelivery.TopicPartitionOffset);
        
            return response ?? OperationResult.Processing();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred while trying to make withdrawal from '{AccountId}' account",
                withdrawTransactionModel.SourceAccountId);
            
            return OperationResult.InternalFail("Error occurred while trying to make withdrawal");
        }
    }
    
    public async Task<OperationResult> Transfer(TwoWayTransactionModel transferTransactionModel)
    {
        try
        {
            if (!ObjectId.TryParse(transferTransactionModel.SourceAccountId, out var sourceId))
            {
                return OperationResult.BadRequest("Source account id has invalid format");
            }
            
            if (!ObjectId.TryParse(transferTransactionModel.DestinationAccountId, out var destinationId))
            {
                return OperationResult.BadRequest("Destination account id has invalid format");
            }
            
            var destinationAccount = await _accountsRepository.GetAsync(destinationId);
            var sourceAccount = await _accountsRepository.GetAsync(sourceId);
            if (sourceAccount == null || !AccountValidator.IsAccountValid(sourceAccount, transferTransactionModel.SourceAccountSecurityCode))
            {
                return OperationResult.BadRequest("Provided account information is not valid. Account is expired or entered " +
                                                  "security code is not correct");
            }
            
            if (destinationAccount == null || !AccountValidator.IsAccountValid(destinationAccount))
            {
                return OperationResult.BadRequest("Destination account information is not valid. Account is probably expired");
            }

            var transactionMessage = transferTransactionModel.ToKafkaMessage();
            var messageDelivery = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
            if (messageDelivery.Status != PersistenceStatus.Persisted)
            {
                _logger.LogError("Unable to publish transfer transaction message into Kafka. Message was not persisted");
                return OperationResult.InternalFail("Error occurred while trying to make transfer");
            }
        
            var response = await _responseService.GetResponse<OperationResult>(transactionMessage, messageDelivery.TopicPartitionOffset);
        
            return response ?? OperationResult.Processing();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred while trying to make transfer from '{AccountId}' account " +
                                        "to '{SourceId}' account",
                transferTransactionModel.SourceAccountId, transferTransactionModel.DestinationAccountId);
            
            return OperationResult.InternalFail("Error occurred while trying to make transfer");
        }
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