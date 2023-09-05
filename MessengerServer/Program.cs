using MessengerServer.Database.Interfaces;
using MessengerServer.Database.Repositories;
using MessengerServer.Middleware;
using MessengerServer.Services;
using MessengerServer.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;

namespace MessengerServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            builder.Services.AddSingleton<DBContext>();

            builder.Services.AddSingleton<IChatRepository, ChatRepository>();
            builder.Services.AddSingleton<IConfirmCodeRepository, ConfirmCodeRepository>();
            builder.Services.AddSingleton<IContactListRepository, ContactListRepository>();
            builder.Services.AddSingleton<IFileMetadataRepository, FileMetadataRepository>();
            builder.Services.AddSingleton<IMessageRepository, MessageRepository>();
            builder.Services.AddSingleton<ISessionRepository, SessionRepository>();
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<IChatListRepository, ChatListRepository>();   

            builder.Services.AddSingleton<IJWTService, JWTService>();
            builder.Services.AddScoped<IMailService, MailService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IFilesProvider, FilesProvider>();

            builder.Services.AddHostedService<CleanerSevice>();

            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            app.UseForwardedHeaders();
            app.UseMiddleware<AuthMiddleware>();
            app.MapControllers();
            app.MapHub<NotificationHub>("/Notification");

            app.Run();
        }
    }
}