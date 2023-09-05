using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public class ChatRepository : Repository<Chat>, IChatRepository
    {
        public ChatRepository(DBContext context) : base(context.Chats) { }

        public async Task<Chat?> GetPersonalChatByUsersAsync(string userId, string contactID)
        {
            var filter = Builders<Chat>.Filter.All(nameof(Chat.MembersIDs), new[] { userId, contactID });
            filter &= Builders<Chat>.Filter.Eq(nameof(Chat.Type), ChatType.Personal);

            var data = await collection.FindAsync(filter);
            return data.SingleOrDefault();
        }

        public async Task<IEnumerable<Chat>> GetUserPersonalChatsAsync(string userId)
        {
            var filter = Builders<Chat>.Filter.In(nameof(Chat.MembersIDs), new[] { userId });
            filter &= Builders<Chat>.Filter.Eq(nameof(Chat.Type), ChatType.Personal);

            var data = await collection.FindAsync(filter);
            return data.ToEnumerable();
        }

        public async Task<IEnumerable<Chat>> GetChatsAsync(int count, string? skipWhileID, params string[] chatIDs)
        {
            if (count <= 0) return Enumerable.Empty<Chat>();

            var filter = Builders<Chat>.Filter.In(nameof(Chat.ID), chatIDs);
            if (skipWhileID is not null)
                filter &= Builders<Chat>.Filter.Lt(nameof(Chat.ID), skipWhileID);

            var sort = Builders<Chat>.Sort.Descending(x => x.ID);
            var chats = await collection.FindAsync(filter, new()
            {
                Sort = sort,
                Limit = count,
            });

            return chats.ToEnumerable();
        }
    }
}
