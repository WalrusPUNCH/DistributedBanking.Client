using DistributedBanking.Client.Data.Repositories.Base;
using DistributedBanking.Client.Data.Services;
using MongoDB.Driver;
using Shared.Data.Entities.Constants;
using Shared.Data.Entities.EndUsers;

namespace DistributedBanking.Client.Data.Repositories.Implementation;

public class CustomersRepository : RepositoryBase<CustomerEntity>, ICustomersRepository
{
    private IMongoDatabase _database;
    
    public CustomersRepository(
        IMongoDbFactory mongoDbFactory) 
        : base(
            mongoDbFactory.GetDatabase(), 
            CollectionNames.EndUsers)
    {
        _database = mongoDbFactory.GetDatabase();
    }
}