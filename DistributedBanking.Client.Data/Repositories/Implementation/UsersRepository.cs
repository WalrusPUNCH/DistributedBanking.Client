using Contracts.Extensions;
using DistributedBanking.Client.Data.Repositories.Base;
using MongoDB.Driver;
using Shared.Data.Entities.Constants;
using Shared.Data.Entities.Identity;
using Shared.Data.Services;

namespace DistributedBanking.Client.Data.Repositories.Implementation;

public class UsersRepository : RepositoryBase<ApplicationUser>, IUsersRepository
{
    private IMongoDatabase _database;
    
    public UsersRepository(
        IMongoDbFactory mongoDbFactory) 
        : base(
            mongoDbFactory.GetDatabase(), 
            CollectionNames.Service.Users)
    {
        _database = mongoDbFactory.GetDatabase();
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return (await GetAsync(u => u.NormalizedEmail == email.NormalizeString())).FirstOrDefault();
    }
}