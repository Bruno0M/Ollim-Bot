using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Ollim.Bot.Services;
using Ollim.Domain.Repositories;

namespace Ollim.Bot.Commands.Modules
{
    public class SendDailyMessageCommandHandler : InteractionModuleBase
    {
        private readonly ISendMessageService _sendService;
        private readonly DiscordSocketClient _client;
        private readonly ILogger<PingCommandHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SendDailyMessageCommandHandler(ISendMessageService sendService, DiscordSocketClient client, ILogger<PingCommandHandler> logger, IServiceProvider serviceProvider)
        {
            _sendService = sendService;
            _client = client;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [SlashCommand("days_to", "Days until 01, 2026!")]
        public async Task SendDailyMessage()
        {
            using var scope = _serviceProvider.CreateScope();
            var _channelRepository = scope.ServiceProvider.GetRequiredService<IChannelRepository>();

            var guild = Context.Guild;
            var channel = await _channelRepository.GetNotification(guild.Id);
            var textChannel = await guild.GetTextChannelAsync(channel.Id);
            if (textChannel == null) return;

            _logger.LogInformation("Aqui: "+ textChannel.ToString());

            await DeferAsync();
            await _sendService.SendDailyMessageAsync(textChannel, Context.Interaction as SocketSlashCommand);
        }
    }
}
