using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Data.Converters;

namespace Shared.Data.Entities;

public abstract class BaseEntity
{
    [BsonId]
    [JsonConverter(typeof(ObjectIdJsonConverter)), JsonPropertyName("_id"), BsonElement(nameof(Id))]
    public ObjectId Id { get; set; }
}