using DistributedBanking.Client.Data.Repositories.Base;
using MongoDB.Driver;
using Shared.Data.Entities;
using Shared.Data.Entities.Constants;
using Shared.Data.Services;

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