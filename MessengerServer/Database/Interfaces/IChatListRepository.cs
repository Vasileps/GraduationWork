namespace MessengerServer.Database.Interfaces
{
    public interface IChatListRepository : IRepository<ChatList>
    {
        public Task AddChatAsync(string chatID, string userID);

        public Task AddChatToManyAsync(string chatID, params string[] userIDs);

        public Task<ChatList?> GetByUserIDAsync(string userID);
    }
}
