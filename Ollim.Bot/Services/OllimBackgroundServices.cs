
namespace Ollim.Bot.Services
{
    public class OllimBackgroundServices : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OllimBackgroundServices(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var messageService = scope.ServiceProvider.GetRequiredService<ISendMessageService>();
                        await messageService.ScheduleMessage(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}
