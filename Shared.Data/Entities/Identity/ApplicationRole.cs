using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Data.Entities.Identity;

public class ApplicationRole : BaseEntity
{
    [BsonElement(nameof(Name))]
    public string Name { get; init; }
    
    [BsonElement(nameof(NormalizedName))]
    public string NormalizedName { get; init; }
    
    public ApplicationRole(string name)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }
}