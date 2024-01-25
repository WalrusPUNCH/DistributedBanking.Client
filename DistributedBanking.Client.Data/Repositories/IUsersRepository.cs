using DistributedBanking.Client.Data.Repositories.Base;
using Shared.Data.Entities.Identity;

namespace DistributedBanking.Client.Data.Repositories;

public interface IUsersRepository : IRepositoryBase<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
}