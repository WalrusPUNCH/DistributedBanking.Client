using Contracts;
using DistributedBanking.Client.Domain.Models.Transaction;

namespace DistributedBanking.Client.Domain.Services;

public interface ITransactionService
{
    Task<OperationStatusModel> Deposit(OneWayTransactionModel depositTransactionModel);
    Task<OperationStatusModel> Withdraw(OneWaySecuredTransactionModel withdrawTransactionModel);
    Task<OperationStatusModel> Transfer(TwoWayTransactionModel transferTransactionModel);
    Task<decimal> GetBalance(string accountId);
    Task<IEnumerable<TransactionResponseModel>> GetAccountTransactionHistory(string accountId);
}