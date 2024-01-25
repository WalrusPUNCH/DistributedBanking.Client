using System.ComponentModel.DataAnnotations;
using Shared.Data.Entities.Constants;

namespace DistributedBanking.API.Models.Account;

public class AccountCreationDto
{
    [Required]
    public required string Name { get; set; }
    
    [Required]
    public required AccountType Type { get; set; }
}