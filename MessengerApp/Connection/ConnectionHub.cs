using MessengerApp.Connection.Schemas;
using MessengerApp.Http;
using System;
using System.IO;
using System.Text.Json;

#nullable enable
namespace MessengerApp.Connection
{
    public static class ConnectionHub
    {
        private const string tokensFileName = "tokens.json";

        private static readonly JsonSerializerOptions serializeOptions = new() { WriteIndented = true };

        private static Uri address;
        private static TokensContainer tokens;
        private static UserInfo? _userInfo;
        public static UserInfo UserInfo
        {
            get
            {
                if (_userInfo is null) throw new Exception("UserInfo not found.\r\nCheck if logged in");
                return _userInfo;
            }
            set => _userInfo = value;
        }

        public static bool Connected { get; private set; }
        public static MessengerHttp Http { get; private set; }
        public static NotificationHub Notifications { get; private set; }

        static ConnectionHub()
        {
            object locker = new();
            lock (locker)
            {
                address = new Uri("http://mirrornet.space:5000/");
#if DEBUG
                address = new Uri("http://localhost:5257/");
#endif
                tokens = new();
                TryLoadTokens();
                tokens.Updated += SaveTokens;

                Http = new MessengerHttp(address, tokens);
                Http.SignedIn += SignedInHandler;
                Http.SignedOut += SignedOutHandler;

                Notifications = new NotificationHub();

                Notifications.ProfileUpdated += (info) => UserInfo = (UserInfo)info;
            }
        }

        private static void SignedInHandler()
        {
            Notifications.Connect(address, tokens);
        }

        private static void SignedOutHandler()
        {
            tokens.Clear();
            Notifications.Disconnect();
            Notifications = new();
        }

        private static void TryLoadTokens()
        {
            var path = Path.Combine(App.DataDirectory.FullName, tokensFileName);
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            tokens.TryParseJson(json);
        }

        public static void SaveTokens()
        {
            var path = Path.Combine(App.DataDirectory.FullName, tokensFileName);

            if (tokens.Valid)
            {
                var jwt = new Tokens(tokens.AccessJWT!, tokens.RefreshJWT!);
                var json = JsonSerializer.Serialize(jwt, serializeOptions);
                File.WriteAllText(path, json);
            }
            else if (File.Exists(path)) File.Delete(path);
        }
    }
}
