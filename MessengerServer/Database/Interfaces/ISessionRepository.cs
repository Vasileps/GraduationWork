namespace MessengerServer.Database.Interfaces
{
    public interface ISessionRepository : IRepository<Session>
    {
        public Task<Session?> GetByAccessTokenAsync(string accessToken);

        public Task<bool> CheckIfTokenTakenAsync(string accessedToken, string refreshToken);

        public Task DeleteOldAsync(TimeSpan lifetime);
    }
}
