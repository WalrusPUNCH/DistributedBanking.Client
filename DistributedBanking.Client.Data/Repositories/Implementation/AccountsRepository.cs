using DistributedBanking.Client.Data.Repositories.Base;
using DistributedBanking.Client.Data.Services;
using MongoDB.Driver;
using Shared.Data.Entities;
using Shared.Data.Entities.Constants;

namespace DistributedBanking.Client.Data.Repositories.Implementation;

public class AccountsRepository : RepositoryBase<AccountEntity>, IAccountsRepository
{
    private IMongoDatabase _database;
    
    public AccountsRepository(IMongoDbFactory mongoDbFactory) 
        : base(
            mongoDbFactory.GetDatabase(), 
            CollectionNames.Accounts) 
    {
        _database = mongoDbFactory.GetDatabase();
    }
}