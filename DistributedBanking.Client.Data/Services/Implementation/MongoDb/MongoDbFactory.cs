using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace DistributedBanking.Client.Data.Services.Implementation.MongoDb;

public class MongoDbFactory : IMongoDbFactory
{
    private readonly string _databaseName;
    private readonly IMongoClient _client;

    public MongoDbFactory(string connectionString, string databaseName)
    {
        _databaseName = databaseName;
        
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
#if DEBUG
        settings.ClusterConfigurator = cb => { 
            cb.Subscribe<CommandStartedEvent>(e => 
            {
                new Logger<MongoDbFactory>(MongoDbDriverLoggerFactory.LoggerFactory)
                    .LogInformation("{CommandName} - {CommandJson}", e.CommandName, e.Command.ToJson());
            });
        };
#endif

        _client = new MongoClient(settings);
    }
    
    public IMongoDatabase GetDatabase()
    {
        return _client.GetDatabase(_databaseName);
    }

    public IMongoClient GetClient()
    {
        return _client;
    }
}