using DistributedBanking.Client.Domain.Models.Transaction;

namespace DistributedBanking.Client.Domain.Services;

public interface ITransactionService
{
    Task<bool> Deposit(OneWayTransactionModel depositTransactionModel);
    Task<bool> Withdraw(OneWaySecuredTransactionModel withdrawTransactionModel);
    Task<bool> Transfer(TwoWayTransactionModel transferTransactionModel);
    Task<decimal> GetBalance(string accountId);
    Task<IEnumerable<TransactionResponseModel>> GetAccountTransactionHistory(string accountId);
}