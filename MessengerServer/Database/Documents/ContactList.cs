using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MessengerServer.Database.Documents
{
    public class ContactList : Entity
    {
        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string[] Contacts { get; set; }

        [BsonConstructor]
        private ContactList() { }

        public ContactList(string userID, params string[] contactsIDs)
        {
            UserID = userID;
            Contacts = contactsIDs;
        }
    }
}
