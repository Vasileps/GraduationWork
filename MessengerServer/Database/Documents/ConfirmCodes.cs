using MongoDB.Bson.Serialization.Attributes;

namespace MessengerServer.Database.Documents
{
    public class ConfirmCode : Entity
    {
        [BsonRequired]
        public string Source { get; set; }

        [BsonRequired]
        public string Code { get; set; }

        [BsonRequired]
        public ConfirmType Type { get; set; }

        [BsonRequired]
        public DateTime CreatedAt { get; set; }

        [BsonIgnoreIfNull]
        public string? AdditionalData { get; set; }

        [BsonConstructor]
        private ConfirmCode() { }

        public ConfirmCode(string source, string code, ConfirmType type, string? additionalData = null)
        {
            Source = source;
            Code = code;
            Type = type;
            AdditionalData = additionalData;
            CreatedAt = DateTime.UtcNow;
        }
    }

    public enum ConfirmType
    {
        ConfirmAccountByMail,
    }
}
