using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Payment;

public class KoFiPayment
{
    [BsonId]
    public string MessageID { get; set; } = string.Empty;

    [BsonElement("raw")]
    public string RawEvent { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("amount")]
    public double Amount { get; set; }

    [BsonElement("handled")]
    public bool Handled { get; set; }
}
