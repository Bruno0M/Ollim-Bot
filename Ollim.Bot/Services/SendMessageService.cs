using Discord;
using Discord.WebSocket;

namespace Ollim.Bot.Services
{
    public class SendMessageService : ISendMessageService
    {
        private DateTime _targetDate;
        private readonly ILogger<SendMessageService> _logger;
        private ITextChannel _textChannel;

        public SendMessageService(ILogger<SendMessageService> logger)
        {
            _targetDate = new DateTime(2026, 6, 1);
            _logger = logger;
        }

        public void SetTextChannel(ITextChannel textChannel)
        {
            _textChannel = textChannel;
        }

        public async Task ScheduleMessage(CancellationToken stoppingToken)
        {
            var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            var brazilTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, brazilTimeZone);
            var scheduledTime = new TimeSpan(7, 0, 0);

            var nextRun = brazilTime.Date.Add(scheduledTime);
            if (brazilTime.TimeOfDay >= scheduledTime)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - brazilTime;
            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            _logger.LogInformation($"Próxima execução agendada para: {nextRun}");

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }

            if (!stoppingToken.IsCancellationRequested && _textChannel != null)
            {
                await SendDailyMessageAsync(_textChannel);
            }
        }

        public async Task SendDailyMessageAsync(ITextChannel textChannel, SocketSlashCommand command = null)
        {
            try
            {
                //_logger.LogInformation($"Enviando mensagem diária às {scheduledTime} (horário de Brasília).");

                DateTime now = DateTime.Now;
                TimeZoneInfo brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                DateTime brazilTime = TimeZoneInfo.ConvertTime(now, brazilTimeZone);
                TimeSpan timeRemaining = _targetDate - brazilTime;

                if (timeRemaining.TotalSeconds > 0)
                {
                    string message = $"Faltam {timeRemaining.Days} dias e {timeRemaining.Hours} horas para {_targetDate:dd/MM/yyyy}!";

                    if (command != null)
                    {
                        await command.ModifyOriginalResponseAsync(props => props.Content = message);
                    }
                    else
                    {
                        await textChannel.SendMessageAsync(message);
                    }
                    _logger.LogInformation($"Mensagem enviada!\n {message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar a mensagem diária.");
            }
        }
    }
}