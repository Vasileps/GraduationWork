using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerServer.Database.Documents
{
    public class Session : Entity
    { 
        [BsonRequired]
        public string AccessToken { get; set; }

        [BsonRequired]
        public string RefreshToken { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        [BsonRequired]
        public DateTime LastRefresh;

        [BsonIgnoreIfNull]
        public string? DeviceInfo { get; set; }

        [BsonConstructor]
        private Session() { }

        public Session(string token, string refreshToken, string userID, string? deviceInfo = null)
        {
            AccessToken = token;
            RefreshToken = refreshToken;
            UserID = userID;
            DeviceInfo = deviceInfo;
            LastRefresh = DateTime.UtcNow;
        }
    }
}
