using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        protected readonly IMongoCollection<TEntity> collection;

        public Repository(IMongoCollection<TEntity> collection)
        {
            this.collection = collection;
        }

        public async Task<TEntity> AddAsync(TEntity obj)
        {
            await collection.InsertOneAsync(obj);
            return obj;
        }

        public async Task DeleteAsync(string id)
        {
            await collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(nameof(Entity.ID), id));
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var all = await collection.FindAsync(Builders<TEntity>.Filter.Empty);
            return all.ToEnumerable();
        }

        public async Task<TEntity?> GetByIDAsync(string id)
        {
            var data = await collection.FindAsync(Builders<TEntity>.Filter.Eq(nameof(Entity.ID), id));
            return data.SingleOrDefault();
        }

        public async Task<IEnumerable<TEntity>> GetManyByIDsAsync(params string[] id)
        {
            var filter = Builders<TEntity>.Filter.In(nameof(Entity.ID), id);
            var data = await collection.FindAsync<TEntity>(filter);

            return data.ToEnumerable();
        }

        public async Task UpdateAsync(TEntity obj)
        {
            var result = await collection.ReplaceOneAsync(Builders<TEntity>.Filter.Eq(nameof(Entity.ID), obj.ID), obj);
            if (!result.IsAcknowledged) throw new Exception("UpdateIsNotAcknowledged");
        }
    }
}
