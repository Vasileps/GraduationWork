namespace MessengerServer.Database.Interfaces
{
    public interface IMessageRepository : IRepository<Message>
    {
        public Task<IEnumerable<Message>> GetMessagesDescendingAsync(string chatID, int count, string? skipWhileID);

        public Task<IEnumerable<Message>> GetMessagesAscendingAsync(string chatID, int count, string? skipWhileID);

        public Task UpdateSenderUsernameAsync(string senderID, string senderUsername);
    }
}
