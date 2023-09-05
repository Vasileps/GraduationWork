using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public class SessionRepository : Repository<Session>, ISessionRepository
    {
        public SessionRepository(DBContext context) : base(context.Sessions) { }

        public async Task<bool> CheckIfTokenTakenAsync(string accessedToken, string refreshToken)
        {
            var filter = Builders<Session>.Filter.Eq(nameof(Session.AccessToken), accessedToken);
            filter |= Builders<Session>.Filter.Eq(nameof(Session.RefreshToken), refreshToken);

            var data = await collection.FindAsync(filter);
            return data.Any();
        }

        public async Task DeleteOldAsync(TimeSpan lifetime)
        {
            var expireDate = DateTime.UtcNow - lifetime;
            var filter = Builders<Session>.Filter.Lt(x => x.LastRefresh, expireDate);

            await collection.DeleteManyAsync(filter);
        }

        public async Task<Session?> GetByAccessTokenAsync(string accessToken)
        {
            var data = await collection.FindAsync(Builders<Session>.Filter.Eq(nameof(Session.AccessToken), accessToken));
            return data.SingleOrDefault();
        }
    }
}
