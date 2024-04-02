namespace DistributedBanking.Client.Domain.Models.Identity;

public class ShortWorkerModelResponse : ShortUserModelResponse
{
    public required string Position { get; set; }

    public required AddressModel Address { get; set; }
}