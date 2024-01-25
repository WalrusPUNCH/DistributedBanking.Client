using MongoDB.Bson.Serialization.Attributes;
using Shared.Data.Entities.Constants;

namespace Shared.Data.Entities;

public class AccountEntity : BaseEntity
{
    [BsonElement(nameof(Name))]
    public required string Name { get; set; }
    
    [BsonElement(nameof(Type))]
    public AccountType Type { get; set; }
    
    [BsonElement(nameof(Balance))]
    public decimal Balance { get; set; }
    
    [BsonElement(nameof(ExpirationDate))]
    public DateTime ExpirationDate { get; set; }
    
    [BsonElement(nameof(SecurityCode))]
    public required string SecurityCode { get; set; }
    
    [BsonElement(nameof(Owner))]
    public string? Owner { get; set; }
    
    [BsonElement(nameof(CreatedAt))]
    public DateTime CreatedAt { get; set; }
}