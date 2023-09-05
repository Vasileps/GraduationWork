namespace MessengerServer.Database.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<IEnumerable<User>> SearchByUsernameAsync(string username, int count, string? skipWhileID, params string[] ignoreID);

        public Task<User?> GetByMailAsync(string mail);

        public Task<User?> GetByUsernameAsync(string username);
    }
}
