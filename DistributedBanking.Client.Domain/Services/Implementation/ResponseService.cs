using System.Reactive.Linq;
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

    public Task<T?> GetResponse<T>(MessageBase messageBase, TopicPartitionOffset messageOffset)
    {
        var channel = BuildResponseChannel(messageBase, messageOffset);
        

        var ct = new CancellationTokenSource();
        ct.CancelAfter(2000);

        T? result = default;
        ct.Token.Register(() => { _redisSubscriber.Unsub(channel); });
        _redisSubscriber.ObserveChannel<T>(channel).SingleAsync().Subscribe(onNext: (r) => result = r, ct.Token);
        
        //var y = await _redisSubscriber.SingleObserveChannel<T>(channel);

        if (result is null)
        {
            _logger.LogError("Unable to get response from Redis channel '{Channel}'", channel);
        }
        
        return Task.FromResult(result);
        
            /*.SingleOrDefaultAsync()
            .Subscribe(
                onNext: obj => { }, 
                token: ct.Token)*/;
        
        
    }
    
    public async Task<T?> GetResponse2<T>(MessageBase messageBase, TopicPartitionOffset messageOffset)
    {
        var channel = BuildResponseChannel(messageBase, messageOffset);
        
        const int timeout = 2000;
        var task = _redisSubscriber.SingleObserveChannel<T>(channel);
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
        {
            // Task completed within timeout.
            // Consider that the task may have faulted or been canceled.
            // We re-await the task so that any exceptions/cancellation is rethrown.
            return await task;
        }
        else
        {
            // timeout/cancellation logic
            return default;
        }
        
    }

    private static string BuildResponseChannel(MessageBase messageBase, TopicPartitionOffset messageOffset)
    {
        return $"{messageBase.ResponseChannelPattern}:{messageOffset.Partition}:{messageOffset.Offset}";
    }
}