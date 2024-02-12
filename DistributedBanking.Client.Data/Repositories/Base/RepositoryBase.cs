using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Data.Entities;
using System.Linq.Expressions;

namespace DistributedBanking.Client.Data.Repositories.Base;

public class RepositoryBase<T> : IRepositoryBase<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> Collection;
    private readonly FilterDefinitionBuilder<T> _filterBuilder = Builders<T>.Filter;
    private readonly MongoCollectionSettings _mongoCollectionSettings = new() { GuidRepresentation = GuidRepresentation.Standard };
    
    protected RepositoryBase(
        IMongoDatabase database, 
        string collectionName)
    {
       
        if (!database.ListCollectionNames().ToList().Contains(collectionName))
        {
            database.CreateCollection(collectionName);
        }
        
        Collection = database.GetCollection<T>(collectionName, _mongoCollectionSettings);
    }
    
    public virtual async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        Collection.FindOneAndUpdate(_filterBuilder.Empty, null);
        return await Collection.Find(_filterBuilder.Empty).ToListAsync();
    }

    public virtual async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
    {
        return await Collection.Find(filter).ToListAsync();
    }

    public virtual async Task<T?> GetAsync(ObjectId id)
    {
        var filter = _filterBuilder.Eq(e => e.Id, id);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? filter)
    {
        return await Collection.Find(filter ?? FilterDefinition<T>.Empty).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        
        await Collection.InsertOneAsync(entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        
        var filter = _filterBuilder.Eq(e => e.Id, entity.Id);

        await Collection.ReplaceOneAsync(filter, entity);
    }

    public virtual async Task RemoveAsync(ObjectId id)
    {
        var filter = _filterBuilder.Eq(e => e.Id, id);
       
        await Collection.DeleteOneAsync(filter);
    }
}