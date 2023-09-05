using MessengerApp.Connection.Schemas;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;


namespace MessengerApp.Connection
{
    public class NotificationHub
    {
        private HubConnection connection;
        private List<HoldToken> holdTokens = new();
        private bool holdNotifications;

        private Queue<(Action<object> Action, object obj)> notificationLine = new();

        public event Action<object> ChatCreated;
        public event Action<object> MessageRecieved;
        public event Action<object> MessageDeleted;
        public event Action<object> UserImageUpdated;
        public event Action<object> ChatImageUpdated;
        public event Action<object> MessageUpdated;
        public event Action<object> ProfileUpdated;
        public event Action<object> ChatInfoUpdated;

        private void Subcribe()
        {
            connection.On<Chat>(nameof(ChatCreated), (chat) => HandleNotification(ChatCreated, chat));
            connection.On<Message>(nameof(MessageRecieved), (message) => HandleNotification(MessageRecieved, message));
            connection.On<Message>(nameof(MessageDeleted), (message) => HandleNotification(MessageDeleted, message));
            connection.On<Message>(nameof(MessageUpdated), (info) => HandleNotification(MessageUpdated, info));
            connection.On<UserInfo>(nameof(ProfileUpdated), (info) => HandleNotification(ProfileUpdated, info));
            connection.On<Chat>(nameof(ChatInfoUpdated), (info) => HandleNotification(ChatInfoUpdated, info));
            connection.On<ChatUpdateSchema>(nameof(ChatImageUpdated), (info) => HandleNotification(ChatImageUpdated, info));
            connection.On<ImageUpdateSchema>(nameof(UserImageUpdated), (info) => HandleNotification(UserImageUpdated, info));
        }

        public HoldToken HoldNotifications()
        {
            var token = new HoldToken(this);
            holdTokens.Add(token);
            holdNotifications = true;
            return token;
        }

        private void ReleaseToken(HoldToken token)
        {
            holdTokens.Remove(token);
            if (holdTokens.Count == 0)
            {
                holdNotifications = false;
                ReleaseLine();
            }
        }

        private void ReleaseLine()
        {
            while (notificationLine.TryDequeue(out (Action<object> Action, object obj) notification))
                notification.Action?.Invoke(notification.obj);
        }

        private void HandleNotification(Action<object> action, object obj)
        {
            if (holdNotifications) notificationLine.Enqueue((action, obj));
            else action?.Invoke(obj);
        }

        public async void Connect(Uri address, TokensContainer tokens)
        {
            if (!tokens.Valid) throw new Exception("Invalid Tokens");

            connection = new HubConnectionBuilder()
                .WithUrl(address.ToString() + "Notification", option =>
                {
                    option.Headers.Add("AccessToken", tokens.AccessJWT!);
                })
                .WithAutomaticReconnect()
                .Build();

            notificationLine = new();
            holdNotifications = false;

            Subcribe();
            await connection.StartAsync();
        }

        public async void Disconnect() => await connection.StopAsync();

        public class HoldToken
        {
            private NotificationHub hub;

            public HoldToken(NotificationHub hub)
            {
                this.hub = hub;
            }

            public void Release() => hub.ReleaseToken(this);

            ~HoldToken()
            {
                Release();
            }
        }
    }
}
