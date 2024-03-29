﻿using DistributedBanking.Client.Data.Repositories.Base;
using MongoDB.Driver;
using Shared.Data.Entities.Constants;
using Shared.Data.Entities.Identity;
using Shared.Data.Services;

namespace DistributedBanking.Client.Data.Repositories.Implementation;

public class RolesRepository : RepositoryBase<ApplicationRole>, IRolesRepository
{
    private IMongoDatabase _database;
    
    public RolesRepository(
        IMongoDbFactory mongoDbFactory) 
        : base(
            mongoDbFactory.GetDatabase(), 
            CollectionNames.Service.Roles)
    {
        _database = mongoDbFactory.GetDatabase();
    }
}