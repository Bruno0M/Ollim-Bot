using Discord;

namespace Ollim.Bot.Services
{
    public class SendMessageService : ISendMessageService
    {

        private DateTime _targetDate;
        private readonly ILogger<SendMessageService> _logger;


        public SendMessageService(ILogger<SendMessageService> logger)
        {
            _targetDate = new DateTime(2026, 6, 1);
            _logger = logger;
        }

        public void ScheduleDailyMessage(ITextChannel textChannel)
        {
            TimeSpan timeToFirstRun = GetTimeToNextExecution();

            Timer timer = new Timer(async _ => await SendDailyMessage(textChannel), null, timeToFirstRun, TimeSpan.FromDays(1));
        }

        private async Task SendDailyMessage(ITextChannel textChannel)
        {
            DateTime now = DateTime.Now;
            TimeZoneInfo brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

            DateTime brazilTime = TimeZoneInfo.ConvertTime(now, brazilTimeZone);

            TimeSpan timeRemaining = _targetDate - brazilTime;


            if (timeRemaining.TotalSeconds > 0)
            {
                string message = $"Faltam {timeRemaining.Days} dias e {timeRemaining.Hours} horas para {_targetDate:dd/MM/yyyy}! ";

                await textChannel.SendMessageAsync(message);
                _logger.LogInformation($"Mensagem enviada!\n {message}");

                //ScheduleDailyMessage(textChannel);
            }
        }

        private TimeSpan GetTimeToNextExecution()
        {
            DateTime now = DateTime.Now;
            TimeZoneInfo brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

            DateTime brazilTime = TimeZoneInfo.ConvertTime(now, brazilTimeZone);

            DateTime nextExecution = new DateTime(brazilTime.Year, brazilTime.Month, brazilTime.Day, 7, 0, 0, 0);

            if (brazilTime > nextExecution)
            {
                nextExecution = nextExecution.AddDays(1);
            }

            _logger.Log(LogLevel.Information, $"Agendado para começar as: {nextExecution}");
            return nextExecution - brazilTime;
        }

    }
}