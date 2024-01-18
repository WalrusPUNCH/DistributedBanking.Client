namespace DistributedBanking.Client.Domain.Options;

public record DatabaseOptions(
    string ConnectionString,
    string ReplicaSetConnectionString,
    string DatabaseName);