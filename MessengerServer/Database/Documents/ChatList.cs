using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MessengerServer.Database.Documents
{
    public class ChatList : Entity
    {
        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string[] Chats { get; set; }

        [BsonConstructor]
        private ChatList() { }

        public ChatList(string userID, params string[] chats)
        {
            UserID = userID;
            Chats = chats;
        }
    }
}
