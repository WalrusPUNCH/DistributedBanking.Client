using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Data.Entities.Identity;

public class ApplicationUser : BaseEntity
{
    [BsonElement(nameof(Email))]
    public string Email { get; set; }
    
    [BsonElement(nameof(NormalizedEmail))]
    public string NormalizedEmail { get; set; }
    
    [BsonElement(nameof(PasswordHash))]
    public string PasswordHash { get; set; }
   
    [BsonElement(nameof(PasswordSalt))]
    public string PasswordSalt { get; set; }
    
    [BsonElement(nameof(PhoneNumber))]
    public string PhoneNumber { get; set; }
    
    [BsonElement(nameof(CreatedOn))]
    public DateTime CreatedOn { get; set; }
    
    [BsonElement(nameof(Roles))]
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    
    [BsonElement(nameof(EndUserId))]
    public string EndUserId { get; set; }
}
