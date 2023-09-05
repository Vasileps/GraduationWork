using MessengerServer.Database.Documents;
using MongoDB.Driver;

namespace MessengerServer.Database
{
    public class DBContext
    {
        public MongoClient Client;
        public IMongoDatabase Database;

        public IMongoCollection<User> Users;
        public IMongoCollection<Session> Sessions;
        public IMongoCollection<ConfirmCode> ConfirmCodes;
        public IMongoCollection<ContactList> ContactLists;
        public IMongoCollection<Chat> Chats;
        public IMongoCollection<Message> Messages;
        public IMongoCollection<FileMetadata> FilesMetadata;
        public IMongoCollection<ChatList> ChatLists;

        public DBContext(IConfiguration configuration)
        {
            Client = new MongoClient(configuration.GetValue<string>("MongoDB:ConnectionString"));
            Database = Client.GetDatabase(configuration.GetValue<string>("MongoDB:DatabaseName"));

            BindCollections();
            EnsureUnique();
        }

        private void BindCollections()
        {
            Users = Database.GetCollection<User>(nameof(Users));
            Sessions = Database.GetCollection<Session>(nameof(Sessions));
            ConfirmCodes = Database.GetCollection<ConfirmCode>(nameof(ConfirmCodes));
            ContactLists = Database.GetCollection<ContactList>(nameof(ContactLists));
            Chats = Database.GetCollection<Chat>(nameof(Chats));
            Messages = Database.GetCollection<Message>(nameof(Messages));
            FilesMetadata = Database.GetCollection<FileMetadata>(nameof(FilesMetadata));
            ChatLists = Database.GetCollection<ChatList>(nameof(ChatLists));
        }

        private void EnsureUnique()
        {
            var unique = new CreateIndexOptions { Unique = true };

            Users.Indexes.CreateOne(new CreateIndexModel<User>
                (Builders<User>.IndexKeys.Ascending(nameof(User.Mail)), unique));
            Users.Indexes.CreateOne(new CreateIndexModel<User>
                (Builders<User>.IndexKeys.Ascending(nameof(User.Username)), unique));

            Sessions.Indexes.CreateOne(new CreateIndexModel<Session>
                (Builders<Session>.IndexKeys.Ascending(nameof(Session.AccessToken)), unique));
            Sessions.Indexes.CreateOne(new CreateIndexModel<Session>
                (Builders<Session>.IndexKeys.Ascending(nameof(Session.RefreshToken)), unique));

            ContactLists.Indexes.CreateOne(new CreateIndexModel<ContactList>
                (Builders<ContactList>.IndexKeys.Ascending(nameof(ContactList.UserID)), unique));

            ChatLists.Indexes.CreateOne(new CreateIndexModel<ChatList>
                (Builders<ChatList>.IndexKeys.Ascending(nameof(ChatList.UserID)), unique));

            FilesMetadata.Indexes.CreateOne(new CreateIndexModel<FileMetadata>
                (Builders<FileMetadata>.IndexKeys.Ascending(nameof(FileMetadata.RelativePath)), unique));
        }
    }
}
