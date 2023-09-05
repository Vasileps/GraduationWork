namespace MessengerServer.Database.Interfaces
{
    public interface IContactListRepository : IRepository<ContactList>
    {
        public Task<ContactList?> GetByUserIDAsync(string userID);

        public Task AddContactAsync(string userID, string contactID);
    }
}
