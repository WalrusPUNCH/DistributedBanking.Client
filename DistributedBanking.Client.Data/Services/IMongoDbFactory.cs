using MongoDB.Driver;

namespace DistributedBanking.Client.Data.Services;

public interface IMongoDbFactory
{
    IMongoDatabase GetDatabase();
    IMongoClient GetClient();
}
