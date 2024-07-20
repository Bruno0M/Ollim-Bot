using Discord;
using Discord.WebSocket;

namespace Ollim.Bot.Services
{
    public class SendMessageService : ISendMessageService
    {

        private DateTime _targetDate;

        public SendMessageService()
        {
            _targetDate = new DateTime(2026, 6, 1);
        }

        public void ScheduleDailyMessage(ITextChannel textChannel)
        {
            TimeSpan timeToFirstRun = GetTimeToNextExecution();

            Timer timer = new Timer(_ => SendDailyMessage(textChannel), null, timeToFirstRun, TimeSpan.FromSeconds(4));
        }

        private async void SendDailyMessage(ITextChannel textChannel)
        {
            TimeSpan timeRemaining = _targetDate - DateTime.Now;

            if (timeRemaining.TotalSeconds > 0)
            {
                string message = $"Faltam {timeRemaining.Days} dias para {_targetDate:dd/MM/yyyy}!";

                await textChannel.SendMessageAsync(message);
            }

        }

        private TimeSpan GetTimeToNextExecution()
        {
            // Define o horário de execução para as 7:00 AM do próximo dia
            DateTime now = DateTime.Now;
            DateTime nextExecution = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0, 0);

            if (now > nextExecution)
            {
                nextExecution = nextExecution.AddDays(1);
            }

            return nextExecution - now;
        }

    }
}
