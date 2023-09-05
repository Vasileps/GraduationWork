using MessengerServer.Database.Documents;
using MessengerServer.Database.Interfaces;
using MessengerServer.DataStructures;
using MessengerServer.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MessengerServer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> hub;
        private readonly IUserRepository userRep;
        private readonly IContactListRepository contactsRep;
        private readonly IChatRepository chatRep;
        private readonly IChatListRepository chatListRep;

        public NotificationService(IHubContext<NotificationHub> notificationHub, IUserRepository userRepository,
            IContactListRepository contactListRepository, IChatRepository chatRepository, IChatListRepository chatListRep)
        {
            this.hub = notificationHub;
            this.userRep = userRepository;
            this.contactsRep = contactListRepository;
            this.chatRep = chatRepository;
            this.chatListRep = chatListRep;
        }

        public async Task ChatCreated(Chat chat)
        {
            await ChatNotify(chat, "ChatCreated");
        }

        public async Task MessageDeleted(Message message, params string[] userIDs)
        {
            await MessageNotify(message, "MessageDeleted", userIDs);
        }

        public async Task MessageRecieved(Message message, params string[] userIDs)
        {
            await MessageNotify(message, "MessageRecieved", userIDs);
        }

        public async Task MessageUpdated(Message message, params string[] userIDs)
        {
            await MessageNotify(message, "MessageUpdated", userIDs);
        }

        public async Task UserImageUpdated(string userID)
        {
            var tasks = new List<Task>();
            var contactList = await contactsRep.GetByUserIDAsync(userID);
            if (contactList is null) contactList = await contactsRep.AddAsync(new(userID));

            foreach (var contact in contactList.Contacts)
                tasks.Add(hub.Clients.Group(contact).SendAsync("UserImageUpdated", new { ID = userID }));

            tasks.Add(hub.Clients.Group(userID).SendAsync("UserImageUpdated", new { ID = userID }));

            var chatList = await chatListRep.GetByUserIDAsync(userID);
            if (chatList is null) chatList = await chatListRep.AddAsync(new(userID));

            var chats = await chatRep.GetManyByIDsAsync(chatList.Chats);   
            foreach (var chat in chats)
            {
                var contact = chat.MembersIDs.FirstOrDefault(x => x != userID);
                if (contact is null) continue;
                tasks.Add(hub.Clients.Group(contact).SendAsync("ChatImageUpdated", new { ChatID = chat.ID }));
            }

            await Task.WhenAll(tasks);
        }

        public async Task ProfileUpdated(ProfileInfo info)
        {
            await hub.Clients.Group(info.ID).SendAsync("ProfileUpdated", info);
        }

        public async Task ChatInfoUpdated(Chat chat)
        {
            await ChatNotify(chat, "ChatInfoUpdated");
        }

        private async Task MessageNotify(Message message, string method, params string[] userIDs)
        {
            var tasks = new List<Task>();
            foreach (var user in userIDs)
                tasks.Add(hub.Clients.Groups(user).SendAsync(method, message));

            await Task.WhenAll(tasks);
        }

        private async Task ChatNotify(Chat chat, string method)
        {
            var tasks = new List<Task>();
            foreach (var userID in chat.MembersIDs)
            {
                if (chat.Type is ChatType.Personal)
                {
                    var contactID = chat.MembersIDs.First(x => x != userID);
                    var user = await userRep.GetByIDAsync(contactID);
                    chat.Name = user is null ? "UserNotFound" : user.Username;
                }
                tasks.Add(hub.Clients.Groups(userID).SendAsync(method, chat));
            }
            await Task.WhenAll(tasks);
        }
    }
}
