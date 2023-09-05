using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace MessengerServer.Database.Documents
{
    public class Message : Entity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonRequired]
        public string ChatID { get; set; }

        [BsonIgnoreIfNull]
        public string? Data { get; set; }

        [BsonRequired]
        public MessageType Type { get; set; }

        [BsonRequired]
        public DateTime SendTime { get; set; }

        [BsonRequired]
        public DateTime UpdateTime { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonRequired]
        public string SenderID { get; set; }

        [BsonRequired]
        public string SenderUsername { get; set; }

        [BsonIgnoreIfDefault]
        [BsonDefaultValue(false)]
        public bool IsDeleted { get; set; }

        [BsonConstructor]
        private Message() { }

        private Message(string chatID, string? data, MessageType type, DateTime sendTime, string senderID, 
            string senderUsername, bool isDeleted = false)
        {
            ChatID = chatID;
            Data = data;
            Type = type;
            SendTime = UpdateTime = sendTime;
            SenderID = senderID;
            SenderUsername = senderUsername;            
            IsDeleted = isDeleted;
        }

        public static Message TextMessage(string chatID, string text, DateTime sendTime, string senderID, string senderUsername)
            => new(chatID, text, MessageType.Text, sendTime, senderID, senderUsername);

        public static Message FileMessage(string chatID, FileType fileType, DateTime sendTime, string senderID, string senderUsername)
             => new(chatID, null, (MessageType)fileType, sendTime, senderID, senderUsername);

        public void SetFileMetadata(FileMetadata metadata)
        {
            var data = new Metadata(metadata.FileName, metadata.Size);
            Data = JsonSerializer.Serialize(data);
        }

        private record Metadata(string Filename, long Size);
    }    

    public enum MessageType : ushort
    {
        Text,
        Image = 1,
        File = 2
    }

    public enum FileType : byte
    {
        Image = 1,
        File = 2
    }
}
