using DistributedBanking.Client.Data.Repositories.Base;
using DistributedBanking.Client.Data.Services;
using MongoDB.Driver;
using Shared.Data.Entities.Constants;
using Shared.Data.Entities.EndUsers;

namespace DistributedBanking.Client.Data.Repositories.Implementation;

public class WorkersRepository : RepositoryBase<WorkerEntity>, IWorkersRepository
{
    private IMongoDatabase _database;
    
    public WorkersRepository(
        IMongoDbFactory mongoDbFactory) 
        : base(
            mongoDbFactory.GetDatabase(), 
            CollectionNames.EndUsers)
    {
        _database = mongoDbFactory.GetDatabase();
    }
}