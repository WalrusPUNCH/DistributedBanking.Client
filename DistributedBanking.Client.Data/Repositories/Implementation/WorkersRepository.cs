using DistributedBanking.Client.Data.Repositories.Base;
using MongoDB.Driver;
using Shared.Data.Entities.Constants;
using Shared.Data.Entities.EndUsers;
using Shared.Data.Services;

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