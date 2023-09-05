using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public class ConfirmCodeRepository : Repository<ConfirmCode>, IConfirmCodeRepository
    {
        public ConfirmCodeRepository(DBContext context) : base(context.ConfirmCodes) { }

        public async Task DeleteOldAsync(TimeSpan lifetime)
        {
            var expireDate = DateTime.UtcNow - lifetime;
            var filter = Builders<ConfirmCode>.Filter.Lt(x => x.CreatedAt, expireDate);

            await collection.DeleteManyAsync(filter);
        }

        public async Task<IEnumerable<ConfirmCode>> GetBySourceAsync(string source)
        {
            var filter = Builders<ConfirmCode>.Filter.Eq(nameof(ConfirmCode.Source), source);
            var data = await collection.FindAsync(filter);

            return data.ToEnumerable();
        }
    }
}
