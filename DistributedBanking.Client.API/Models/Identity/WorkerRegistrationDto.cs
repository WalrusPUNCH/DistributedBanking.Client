using System.ComponentModel.DataAnnotations;

namespace DistributedBanking.API.Models.Identity;

public class WorkerRegistrationDto : EndUserRegistrationDto
{
    [Required]
    public required string Position { get; set; }
    
    [Required]
    public required AddressDto Address { get; set; }
}