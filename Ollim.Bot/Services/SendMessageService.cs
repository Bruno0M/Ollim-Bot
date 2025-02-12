using Discord;
using Discord.WebSocket;
using Ollim.Domain.Repositories;

namespace Ollim.Bot.Services
{
    public class SendMessageService : ISendMessageService
    {
        private DateTime _targetDate;
        private readonly ILogger<SendMessageService> _logger;
        private DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;

        public SendMessageService(ILogger<SendMessageService> logger, DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            _targetDate = new DateTime(2026, 6, 1);
            _logger = logger;
            _client = client;
            _serviceProvider = serviceProvider;
        }

        public async Task ScheduleMessage(CancellationToken stoppingToken)
        {
            var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            var brazilTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, brazilTimeZone);
            var scheduledTime = new TimeSpan(16, 41, 0);

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
                return;
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await SendMessagesToAllChannelsAsync();
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

        private async Task SendMessagesToAllChannelsAsync()
        {
            using var scoped = _serviceProvider.CreateScope();
            var _channelRepository = scoped.ServiceProvider.GetRequiredService<IChannelRepository>();

            var channels = await _channelRepository.GetAllNotification();

            foreach (var channel in channels)
            {
                //var guild = _client.GetGuild(channel.GuildId);
                var guild = _client.GetGuild(channel.GuildId);
                if (guild != null)
                {
                    var textChannel = guild.GetTextChannel(channel.Id);
                    if (textChannel != null)
                    {
                        await SendDailyMessageAsync(textChannel);
                    }
                }
                else
                {
                    _logger.LogWarning($"Canal com ID {channel.Id} não encontrado no guild {channel.GuildId}.");
                }
            }
        }

    }
}