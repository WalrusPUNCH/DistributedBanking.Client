namespace DistributedBanking.API.Models.Transaction;

public class OneWaySecuredTransactionDto : OneWayTransactionDto
{
    public required string SecurityCode { get; set; }
}