using DistributedBanking.Client.Data.Repositories.Base;
using Shared.Data.Entities;

namespace DistributedBanking.Client.Data.Repositories;

public interface ITransactionsRepository : IRepositoryBase<TransactionEntity>
{
    Task<IEnumerable<TransactionEntity>> AccountTransactionHistory(string accountId);
}