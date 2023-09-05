using MessengerServer.DataStructures;

namespace MessengerServer.Services.Interfaces
{
    public interface INotificationService
    {
        public Task ChatCreated(Chat chat);

        public Task UserImageUpdated(string userID);

        public Task MessageRecieved(Message message, params string[] userIDs);

        public Task MessageUpdated(Message message, params string[] userIDs);

        public Task MessageDeleted(Message message, params string[] userIDs);

        public Task ProfileUpdated(ProfileInfo info);

        public Task ChatInfoUpdated(Chat chat);
    }
}
