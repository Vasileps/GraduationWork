using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MessengerServer.Database.Documents
{
    public class Chat : Entity
    {
        [BsonRequired]
        public ChatType Type { get; set; }

        [BsonIgnoreIfNull]
        public string? Name { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string[] MembersIDs { get; set; }

        [BsonRequired]
        public DateTime LastUpdate { get; set; }

        [BsonConstructor]
        private Chat() { }

        public static Chat Personal(string firstPersonID, string secondPersonID)
        {
            var chat = new Chat
            {
                Type = ChatType.Personal,
                MembersIDs = new[] { firstPersonID, secondPersonID },
                LastUpdate = DateTime.UtcNow,
            };
            return chat;
        }
    }

    public enum ChatType : ushort
    {
        Personal,
    }
}
