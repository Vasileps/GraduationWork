namespace MessengerServer.Database.Interfaces
{
    public interface IConfirmCodeRepository : IRepository<ConfirmCode>
    {
        public Task<IEnumerable<ConfirmCode>> GetBySourceAsync(string source);

        public Task DeleteOldAsync(TimeSpan lifetime);
    }
}
