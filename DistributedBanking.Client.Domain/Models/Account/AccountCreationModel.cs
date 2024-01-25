using Shared.Data.Entities.Constants;

namespace DistributedBanking.Client.Domain.Models.Account;

public class AccountCreationModel
{
    public required string Name { get; set; }
    public AccountType Type { get; set; }
}