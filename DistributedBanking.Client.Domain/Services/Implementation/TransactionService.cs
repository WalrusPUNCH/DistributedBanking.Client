using Confluent.Kafka;
using Contracts;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Domain.Mapping;
using DistributedBanking.Client.Domain.Models.Transaction;
using Mapster;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Data.Entities.Constants;
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
        
        return await PublishTransactionMessage(depositTransactionModel);
        
        /*var transactionMessage = depositTransactionModel.Adapt<TransactionMessage>();
        var result = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
            
        return result.Status == PersistenceStatus.Persisted;*/
    }
    
    public async Task<bool> Withdraw(OneWaySecuredTransactionModel withdrawTransactionModel)
    {
        var account = await _accountsRepository.GetAsync(new ObjectId(withdrawTransactionModel.SourceAccountId));
        if (account == null || !AccountValidator.IsAccountValid(account, withdrawTransactionModel.SecurityCode))
        {
            return false;
        }

        return await PublishTransactionMessage(withdrawTransactionModel);
        
        /*var transactionMessage = withdrawTransactionModel.Adapt<TransactionMessage>();
        var result = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
            
        return result.Status == PersistenceStatus.Persisted;*/
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

        var transactionMessage = transferTransactionModel.Adapt<TransactionMessage>();
        var result = await _transactionsProducer.ProduceAsync(transactionMessage, transactionMessage.Headers);
            
        return result.Status == PersistenceStatus.Persisted;
    }
    
    private async Task<bool> PublishTransactionMessage(OneWayTransactionModel transactionModel)
    {
        var transactionMessage = transactionModel.Adapt<TransactionMessage>();
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
    
    private async Task<OperationStatusModel> DepositInternal(OneWayTransactionModel depositTransactionModel)
    {
        try
        {
            var account = await _accountsRepository.GetAsync(new ObjectId(depositTransactionModel.SourceAccountId));
            if (!AccountValidator.IsAccountValid(account))
            {
                return OperationStatusModel.Fail("Account is expired.");
            }
            
            account.Balance += depositTransactionModel.Amount;
            await _accountsRepository.UpdateAsync(account);

            var transaction = depositTransactionModel.AdaptToEntity(TransactionType.Deposit);
            await _transactionsRepository.AddAsync(transaction);
            
            return OperationStatusModel.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to perform a deposit for '{SourceAccountId}' account. Try again later", depositTransactionModel.SourceAccountId);
            throw;
        }
    }
    
    private async Task<OperationStatusModel> WithdrawInternal(OneWaySecuredTransactionModel withdrawTransactionModel)
    {
        try
        {
            var account = await _accountsRepository.GetAsync(new ObjectId(withdrawTransactionModel.SourceAccountId));
            if (!AccountValidator.IsAccountValid(account, withdrawTransactionModel.SecurityCode))
            {
                return OperationStatusModel.Fail("Provided account information is not valid. Account is expired or entered " +
                                                 "security code is not correct.");
            }
            
            if (account.Balance < withdrawTransactionModel.Amount)
            {
                return OperationStatusModel.Fail("Insufficient funds. " +
                                                 "The transaction cannot be completed due to a lack of available funds in the account.");
            }
            
            account.Balance -= withdrawTransactionModel.Amount;
            await _accountsRepository.UpdateAsync(account);

            var transaction = withdrawTransactionModel.AdaptToEntity(TransactionType.Withdrawal);
            await _transactionsRepository.AddAsync(transaction);
            
            return OperationStatusModel.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to perform a withdrawal from '{SourceAccountId}' account. Try again later", withdrawTransactionModel.SourceAccountId);
            throw;
        }
    }
    
    private async Task<OperationStatusModel> TransferInternal(TwoWayTransactionModel transferTransactionModel)
    {
        try
        {
            var destinationAccount = await _accountsRepository.GetAsync(new ObjectId(transferTransactionModel.DestinationAccountId));
            var sourceAccount = await _accountsRepository.GetAsync(new ObjectId(transferTransactionModel.SourceAccountId));
            if (!AccountValidator.IsAccountValid(sourceAccount, transferTransactionModel.SourceAccountSecurityCode))
            {
                return OperationStatusModel.Fail("Provided account information is not valid. Account is expired or entered " +
                                                   "security code is not correct.");
            }
            
            if (!AccountValidator.IsAccountValid(destinationAccount))
            {
                return OperationStatusModel.Fail("Destination account information is not valid. Account is probably expired.");
            }
            
            if (sourceAccount.Balance < transferTransactionModel.Amount)
            {
                return OperationStatusModel.Fail("Insufficient funds. " +
                                                   "The transaction cannot be completed due to a lack of available funds in the account.");
            }
            
            sourceAccount.Balance -= transferTransactionModel.Amount;
            destinationAccount.Balance += transferTransactionModel.Amount;
            
            await _accountsRepository.UpdateAsync(sourceAccount);
            await _accountsRepository.UpdateAsync(destinationAccount);

            var transaction = transferTransactionModel.AdaptToEntity(TransactionType.Transfer);
            await _transactionsRepository.AddAsync(transaction);
            
            return OperationStatusModel.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to perform a transfer from '{SourceAccountId}' account to '{DestinationAccountId}' account. Try again later", 
                transferTransactionModel.SourceAccountId, transferTransactionModel.DestinationAccountId);
            throw;
        }
    }
}