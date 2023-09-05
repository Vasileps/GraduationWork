using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public class ChatListRepository : Repository<ChatList>, IChatListRepository
    {
        public ChatListRepository(DBContext dBContext) : base(dBContext.ChatLists) { }

        public async Task AddChatAsync(string chatID, string userID)
        {
            var filter = Builders<ChatList>.Filter.Eq(nameof(ChatList.UserID), userID);
            var update = Builders<ChatList>.Update.Push(nameof(ChatList.Chats), chatID);

            await collection.UpdateOneAsync(filter, update);
        }

        public async Task AddChatToManyAsync(string chatID, params string[] userIDs)
        {
            var bulk = new List<UpdateOneModel<ChatList>>();
            foreach (var userID in userIDs) 
            {
                bulk.Add(new(
                    Builders<ChatList>.Filter.Eq(nameof(ChatList.UserID), userID),
                    Builders<ChatList>.Update.Push(nameof(ChatList.Chats), chatID)));
            }

            await collection.BulkWriteAsync(bulk);
        }

        public async Task<ChatList?> GetByUserIDAsync(string userID)
        {
            var filter = Builders<ChatList>.Filter.Eq(nameof(ChatList.UserID), userID);
            var data = await collection.FindAsync(filter);

            return data.SingleOrDefault();
        }
    }
}
