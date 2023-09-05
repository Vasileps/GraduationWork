using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(DBContext context) : base(context.Messages) { }

        public async Task UpdateSenderUsernameAsync(string senderID, string senderUsername)
        {
            var filter = Builders<Message>.Filter.Eq(x => x.SenderID, senderID);
            var update = Builders<Message>.Update.Set(x => x.SenderUsername, senderUsername);

            await collection.UpdateManyAsync(filter, update);
        }

        public async Task<IEnumerable<Message>> GetMessagesAscendingAsync(string chatID, int count, string? skipWhileID)
        {
            return await GetMessagesAsync(chatID, count, skipWhileID, false);
        }

        public async Task<IEnumerable<Message>> GetMessagesDescendingAsync(string chatID, int count, string? skipWhileID)
        {
            return await GetMessagesAsync(chatID, count, skipWhileID, true);
        }

        private async Task<IEnumerable<Message>> GetMessagesAsync(string chatID, int count, string? skipWhileID, bool descending)
        {
            if (count <= 0) return Enumerable.Empty<Message>();

            var sort = descending ?
                Builders<Message>.Sort.Descending(x => x.ID) :
                Builders<Message>.Sort.Ascending(x => x.ID);

            var filter = Builders<Message>.Filter.Eq(x => x.ChatID, chatID);
            filter &= Builders<Message>.Filter.Ne(x => x.IsDeleted, true);

            if (skipWhileID is not null)
                filter &= descending ?
                    Builders<Message>.Filter.Lt(x => x.ID, skipWhileID) :
                    Builders<Message>.Filter.Gt(x => x.ID, skipWhileID);

            var data = await collection.FindAsync(filter, new()
            {
                Sort = sort,
                Limit = count,
            });

            return data.ToEnumerable();
        }
    }
}
