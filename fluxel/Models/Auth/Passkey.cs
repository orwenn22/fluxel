using Fido2NetLib.Objects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Auth;

public class Passkey
{
    [BsonId]
    public ObjectId ObjectID { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("id")]
    public byte[] ID { get; init; } = null!;

    [BsonElement("user")]
    public long UserID { get; init; }

    [BsonElement("handle")]
    public byte[] UserHandle { get; init; } = null!;

    [BsonElement("key")]
    public byte[] PublicKey { get; init; } = null!;

    [BsonElement("count")]
    public uint SignCount { get; set; }

    [BsonElement("descriptor")]
    public PublicKeyCredentialDescriptor Descriptor { get; init; } = null!;
}
