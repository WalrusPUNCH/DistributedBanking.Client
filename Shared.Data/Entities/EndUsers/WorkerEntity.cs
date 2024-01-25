using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Data.Entities.EndUsers;

public class WorkerEntity : EndUserEntityBase
{
    [BsonElement(nameof(Position))]
    public required string Position { get; set; }
    
    [BsonElement(nameof(Address))]
    public required Address Address { get; set; }
}

public class Address
{
    [BsonElement(nameof(Country))]
    public required string Country { get; set; }
    
    [BsonElement(nameof(Region))]
    public required string Region { get; set; }
    
    [BsonElement(nameof(City))]
    public required string City { get; set; }
    
    [BsonElement(nameof(Street))]
    public required string Street { get; set; }
    
    [BsonElement(nameof(Building))]
    public required string Building { get; set; }
    
    [BsonElement(nameof(PostalCode))]
    public required string PostalCode { get; set; }
}