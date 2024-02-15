using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Messages;
using Shared.Redis.Services;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class ResponseService : IResponseService
{
    private readonly IRedisSubscriber _redisSubscriber;
    private readonly ILogger<ResponseService> _logger;

    public ResponseService(
        IRedisSubscriber redisSubscriber,
        ILogger<ResponseService> logger)
    {
        _redisSubscriber = redisSubscriber;
        _logger = logger;
    }
    
    public async Task<T?> GetResponse<T>(MessageBase messageBase, TopicPartitionOffset messageOffset, TimeSpan? timeout = null)
    {
        var channel = BuildResponseChannel(messageBase, messageOffset);
        var timeoutMilliseconds = timeout.HasValue ? Convert.ToInt32(timeout.Value.TotalMilliseconds) : 5000;
        
        var responseTask = _redisSubscriber.SingleObserveChannel<T>(channel);
        if (await Task.WhenAny(responseTask, Task.Delay(timeoutMilliseconds)) == responseTask)
        {
            // Task completed within timeout.
            // Consider that the task may have faulted or been canceled.
            // We re-await the task so that any exceptions/cancellation is rethrown.
            return await responseTask;
        }

        // timeout/cancellation logic
        return default;
    }

    private static string BuildResponseChannel(MessageBase messageBase, TopicPartitionOffset messageOffset)
    {
        return $"{messageBase.ResponseChannelPattern}:{messageOffset.Partition}:{messageOffset.Offset}";
    }
}