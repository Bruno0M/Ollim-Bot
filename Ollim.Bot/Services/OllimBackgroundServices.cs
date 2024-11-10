
namespace Ollim.Bot.Services
{
    public class OllimBackgroundServices : BackgroundService
    {
        private readonly ISendMessageService _messageService;

        public OllimBackgroundServices(ISendMessageService messageService)
        {
            _messageService = messageService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
               await _messageService.ScheduleMessage(stoppingToken);
            }
        }
    }
}
