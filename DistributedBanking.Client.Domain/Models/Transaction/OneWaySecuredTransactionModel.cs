namespace DistributedBanking.Client.Domain.Models.Transaction;

public class OneWaySecuredTransactionModel : OneWayTransactionModel
{
    public string SecurityCode { get; set; }
}