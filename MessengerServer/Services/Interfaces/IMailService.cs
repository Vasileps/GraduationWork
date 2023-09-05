namespace MessengerServer.Services.Interfaces
{
    public interface IMailService
    {
        public Task SendTextAsync(string receiverName, string reciverEmail, string text);

        public Task SendHtmlAsync(string receiverName, string reciverEmail, string html);
    }
}
