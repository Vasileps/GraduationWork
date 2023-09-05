using MailKit.Net.Smtp;
using MimeKit;
using IMailService = MessengerServer.Services.Interfaces.IMailService;

namespace MessengerServer.Services
{
    public class MailService : IMailService
    {
        private MailboxAddress sender;
        private SmtpClient client;

        public MailService(IConfiguration configuration)
        {
            var host = configuration.GetValue<string>("Email:SMTP:Host")!;
            var port = configuration.GetValue<int>("Email:SMTP:Port")!;
            var username = configuration.GetValue<string>("Email:Username")!;
            var password = configuration.GetValue<string>("Email:Password")!;

            client = new SmtpClient();
            client.Connect(host, port, true);
            client.Authenticate(username, password);

            sender = new MailboxAddress("MyMessengerName", username);
        }

        public async Task SendHtmlAsync(string receiverName, string reciverEmail, string html)
        {
            throw new NotImplementedException();
        }

        public async Task SendTextAsync(string receiverName, string reciverEmail, string text)
        {
            MailboxAddress receiver = new MailboxAddress(receiverName, reciverEmail);

            MimeMessage message = new MimeMessage();
            message.From.Add(sender);
            message.To.Add(receiver);

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = text;
            message.Body = bodyBuilder.ToMessageBody();

            await SendEmail(message);
        }

        private async Task SendEmail(MimeMessage message)
        {
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
