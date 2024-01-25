using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace Shared.Data.Converters;

public class ObjectIdJsonConverter : JsonConverter<ObjectId>
{
    public override ObjectId Read(
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options) => new(reader.GetString()!);

    public override void Write(
        Utf8JsonWriter writer,
        ObjectId objectId,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(objectId.ToString());
    }
}