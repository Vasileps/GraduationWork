using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public class ContactListRepository : Repository<ContactList>, IContactListRepository
    {
        public ContactListRepository(DBContext context) : base(context.ContactLists) { }

        public async Task AddContactAsync(string userID, string contactID)
        {
            var filter = Builders<ContactList>.Filter.Eq(nameof(ContactList.UserID), userID);
            var update = Builders<ContactList>.Update.Push(nameof(ContactList.Contacts), contactID);

            await collection.UpdateOneAsync(filter, update);
        }

        public async Task<ContactList?> GetByUserIDAsync(string userID)
        {
            var filter = Builders<ContactList>.Filter.Eq(nameof(ContactList.UserID), userID);
            var data = await collection.FindAsync(filter);

            return data.SingleOrDefault();
        }
    }
}
