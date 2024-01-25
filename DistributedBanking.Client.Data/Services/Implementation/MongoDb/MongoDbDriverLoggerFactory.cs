using Microsoft.Extensions.Logging;

namespace DistributedBanking.Client.Data.Services.Implementation.MongoDb;

public static class MongoDbDriverLoggerFactory
{
    private static ILoggerFactory? _factory;
    public static ILoggerFactory LoggerFactory
    {
        get
        {
            if (_factory != null)
            {
                return _factory;
            }
            
            _factory = ConfigureLogger();
            
            return _factory;
        }
    }

    private static ILoggerFactory ConfigureLogger()
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }));
    }
}