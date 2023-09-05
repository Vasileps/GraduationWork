using MessengerServer.Database.Interfaces;
using Timer = System.Timers.Timer;

namespace MessengerServer.Services
{
    public class CleanerSevice : IHostedService
    {
        private static readonly TimeSpan interval = TimeSpan.FromHours(1);

        private readonly TimeSpan gap = TimeSpan.FromMinutes(5);
        private readonly ISessionRepository sessionRepository;
        private readonly IConfirmCodeRepository confirmCodeRepository;
        private Timer cleaner;

        public CleanerSevice(ISessionRepository sessionRepository, IConfirmCodeRepository confirmCodeRepository)
        {
            this.sessionRepository = sessionRepository;
            this.confirmCodeRepository = confirmCodeRepository;

            cleaner = new Timer();
            cleaner.Interval = interval.TotalSeconds;
            cleaner.AutoReset = true;
            cleaner.Elapsed += Clean;            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cleaner.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cleaner.Stop();
            return Task.CompletedTask;
        }

        private async void Clean(object? sender, EventArgs e)
        {
            await sessionRepository.DeleteOldAsync(GlobalValues.RefreshTokenLifetime + gap);
            await confirmCodeRepository.DeleteOldAsync(GlobalValues.CodeLifetime);
        }
    }
}
