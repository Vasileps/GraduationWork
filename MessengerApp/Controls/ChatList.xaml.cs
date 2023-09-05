using MessengerApp.Connection.Schemas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для ChatList.xaml
    /// </summary>
    public partial class ChatList : UserControl
    {
        private const int chatsAtATime = 15;
        private ObservableCollection<ChatPanel> collection = new();
        private Dictionary<string, ChatPanel> panelsDicitonary = new();
        private ChatPanel selectedChat;

        public event Action<Chat> ChatSelected;

        public ChatList()
        {
            InitializeComponent();
            ChatsControl.ItemsSource = collection;

            var token = ConnectionHub.Notifications.HoldNotifications();
            GetChats();
            ConnectionHub.Notifications.ChatCreated += ChatCreatedHandler;
            ConnectionHub.Notifications.MessageRecieved += MessageRecievedHandler;
            token.Release();
        }

        public async void GetChats()
        {
            var schema = new GetChatsSchema(chatsAtATime, null);

        Repeat:
            var response = await ConnectionHub.Http.GetChatsAsync(schema);
            if (!response.Success) return; //Add exceptionHandler

            foreach (var chat in response.Data!) AddPanel(chat);
            if (response.Data!.Length >= chatsAtATime)
            {
                schema = new GetChatsSchema(chatsAtATime, response.Data![^1].ID);
                goto Repeat;
            }
        }

        private void MessageRecievedHandler(object obj)
        {
            var message = obj as Message;
            if (message is null) return;

            if (!panelsDicitonary.ContainsKey(message.ChatID)) return;

            var view = panelsDicitonary[message.ChatID];
            collection.Remove(view);
            collection.Insert(0, view);

            if (selectedChat is not null)
                if (selectedChat != view)
                    view.Indicator.Visibility = Visibility.Visible;

            if (message.SenderID == ConnectionHub.UserInfo.ID) return;

            var title = panelsDicitonary[message.ChatID].Schema.Name;
            var content = new NotificationMessage(title, message.ToDisplayableString());
            App.NotificationManager.ShowAsync<NotificationMessage>(content);

        }

        private void ChatCreatedHandler(object obj)
        {
            var chat = obj as Chat;
            if (chat is null) return;
            if (!panelsDicitonary.ContainsKey(chat.ID)) AddPanel(chat);
        }

        private void AddPanel(Chat chat)
        {
            var panel = new ChatPanel(chat);
            panel.MouseDown += (s, e) =>
            {
                panel.Indicator.Visibility = Visibility.Collapsed;
                selectedChat = panel;
                ChatSelected?.Invoke(panel.Schema);
            };
            panelsDicitonary.Add(chat.ID, panel);
            collection.Add(panel);
        }
    }
}
