namespace DistributedBanking.API.Models.Identity;

public class ShortWorkerModelDto : ShortUserModelDto
{
    public required string Position { get; set; }

    public required AddressDto Address { get; set; }
}