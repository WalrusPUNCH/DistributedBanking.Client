using Confluent.Kafka;
using Shared.Messaging.Messages;

namespace DistributedBanking.Client.Domain.Services;

public interface IResponseService
{
    Task<T?> GetResponse<T>(MessageBase messageBase, TopicPartitionOffset messageOffset, TimeSpan? timeout = null);
}