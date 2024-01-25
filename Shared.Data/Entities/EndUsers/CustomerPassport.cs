using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Data.Entities.EndUsers;

public class CustomerPassport
{
    [BsonElement(nameof(DocumentNumber))]
    public required string DocumentNumber { get; set; }
    
    [BsonElement(nameof(Issuer))]
    public required string Issuer { get; set; }
    
    [BsonElement(nameof(IssueDateTime))]
    public required DateTime IssueDateTime { get; set; }
    
    [BsonElement(nameof(ExpirationDateTime))]
    public required DateTime ExpirationDateTime { get; set; }
}