namespace MessengerServer.Database.Interfaces
{
    public interface IRepository<TEntity> where TEntity : Entity
    {
        public Task<TEntity> AddAsync(TEntity obj);

        public Task<TEntity?> GetByIDAsync(string id);

        public Task<IEnumerable<TEntity>> GetManyByIDsAsync(params string[] id);

        public Task<IEnumerable<TEntity>> GetAllAsync();

        public Task UpdateAsync(TEntity obj);

        public Task DeleteAsync(string id);
    }
}
