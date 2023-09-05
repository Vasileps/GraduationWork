using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;

namespace MessengerServer.Database.Documents
{
    public class User : Entity
    {
        [BsonRequired]
        public string Username { get; set; }

        [BsonRequired]
        public string Mail { get; set; }

        [BsonRequired]
        public string Password { get; set; }

        [BsonRequired]
        public DateTime CreatedTime { get; set; }

        [BsonDefaultValue(true)]
        [BsonIgnoreIfDefault]
        public bool Verified { get; set; }

        [BsonConstructor]
        private User() { }

        public User(string username, string mail, string password)
        {
            if (username is null or "")
                throw new ArgumentNullException(nameof(username));
            if (mail is null or "")
                throw new ArgumentNullException(nameof(mail));
            if (password is null or "")
                throw new ArgumentNullException(nameof(password));

            Username = username;
            Mail = mail;
            CreatedTime = DateTime.UtcNow;
            Verified = false;

            var passwordHash = SHA256.HashData(Encoding.UTF8.GetBytes(password + CreatedTime.ToString("HH:mm:ss dd.MM.yyyy")));
            Password = Convert.ToBase64String(passwordHash);
        }

        public bool ComparePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            var passwordHash = SHA256.HashData(Encoding.UTF8.GetBytes(password + CreatedTime.ToString("HH:mm:ss dd.MM.yyyy")));
            var passwordString = Convert.ToBase64String(passwordHash);
            return passwordString == Password;
        }

        public void UpdatePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            var passwordHash = SHA256.HashData(Encoding.UTF8.GetBytes(password + CreatedTime.ToString("HH:mm:ss dd.MM.yyyy")));
            Password = Convert.ToBase64String(passwordHash);
        }
    }
}
