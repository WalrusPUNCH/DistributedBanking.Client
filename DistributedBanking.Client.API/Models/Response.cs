using Contracts.Models;

namespace DistributedBanking.API.Models;

public record Response(OperationStatus Status, string Message);

public record Response<T>(OperationStatus Status, string Message, T? Value) : Response(Status, Message);