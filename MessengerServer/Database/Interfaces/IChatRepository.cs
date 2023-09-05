namespace MessengerServer.Database.Interfaces
{
    public interface IChatRepository : IRepository<Chat>
    {
        public Task<IEnumerable<Chat>> GetChatsAsync(int count, string? skipWhileID, params string[] chatIDs);
    }
}
