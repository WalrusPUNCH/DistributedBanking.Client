using DistributedBanking.Client.Data.Repositories.Base;
using DistributedBanking.Client.Data.Services;
using MongoDB.Driver;
using Shared.Data.Entities;
using Shared.Data.Entities.Constants;

namespace DistributedBanking.Client.Data.Repositories.Implementation;

public class TransactionsRepository : RepositoryBase<TransactionEntity>, ITransactionsRepository
{
    private IMongoDatabase _database;
    
    public TransactionsRepository(
        IMongoDbFactory mongoDbFactory) 
        : base(
            mongoDbFactory.GetDatabase(), 
            CollectionNames.Transactions)
    {
        _database = mongoDbFactory.GetDatabase();
    }

    public async Task<IEnumerable<TransactionEntity>> AccountTransactionHistory(string accountId)
    {
        return await Collection
            .Find(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId)
            .SortByDescending(t => t.DateTime)
            .ToListAsync();
    }
}