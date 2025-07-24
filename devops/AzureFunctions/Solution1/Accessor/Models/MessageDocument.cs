using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AccessorService.Models;

public class MessageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("body")]
    public string Body { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}