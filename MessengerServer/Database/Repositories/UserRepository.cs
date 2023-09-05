using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DBContext context) : base(context.Users) { }

        public async Task<IEnumerable<User>> SearchByUsernameAsync(string username, int count, string? skipWhileID,
            params string[] ignoreIDs)
        {
            if (count <= 0) return Enumerable.Empty<User>();

            var regex = $"^(?i)({username})\\w*$";

            var filter = Builders<User>.Filter.Regex(nameof(User.Username), regex);
            if (skipWhileID is not null)
                filter &= Builders<User>.Filter.Lt(nameof(User.ID), skipWhileID);

            foreach (var ignoreID in ignoreIDs)
                filter &= Builders<User>.Filter.Ne(nameof(User.ID), ignoreID);

            var sort = Builders<User>.Sort.Descending(x => x.ID);

            var users = await collection.FindAsync(filter, new()
            {
                Sort = sort,
                Limit = count,
            });

            return users.ToEnumerable();
        }

        public async Task<User?> GetByMailAsync(string mail)
        {
            var data = await collection.FindAsync(Builders<User>.Filter.Eq(nameof(User.Mail), mail));
            return data.SingleOrDefault();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var data = await collection.FindAsync(Builders<User>.Filter.Eq(nameof(User.Username), username));
            return data.SingleOrDefault();
        }
    }
}
