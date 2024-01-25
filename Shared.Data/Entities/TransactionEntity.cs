using MongoDB.Bson.Serialization.Attributes;
using Shared.Data.Entities.Constants;

namespace Shared.Data.Entities;

public class TransactionEntity : BaseEntity
{
    [BsonElement(nameof(SourceAccountId))]
    public required string SourceAccountId { get; set; }
    
    [BsonElement(nameof(DestinationAccountId))]
    public string? DestinationAccountId { get; set; }
    
    [BsonElement(nameof(Type))]
    public required TransactionType Type { get; set; }
    
    [BsonElement(nameof(Amount))]
    public decimal Amount { get; set; }
    
    [BsonElement(nameof(DateTime))]
    public DateTime DateTime { get; set; }
    
    [BsonElement(nameof(Description))]
    public string? Description { get; set; }
}