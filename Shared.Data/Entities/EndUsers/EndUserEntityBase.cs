using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Data.Entities.EndUsers;

//[BsonDiscriminator(RootClass = true)]
//[BsonKnownTypes(typeof(CustomerEntity), typeof(WorkerEntity))]
public abstract class EndUserEntityBase : BaseEntity
{
    [BsonElement(nameof(FirstName))]
    public required string FirstName { get; set; }
    
    [BsonElement(nameof(LastName))]
    public required string LastName { get; set; }
    
    [BsonElement(nameof(BirthDate))]
    public required DateTime BirthDate { get; set; }
    
    [BsonElement(nameof(PhoneNumber))]
    public required string PhoneNumber { get; set; }
    
    [BsonElement(nameof(Email))]
    public required string Email { get; set; }
}