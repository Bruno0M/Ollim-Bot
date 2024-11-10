using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Ollim.Bot.Commands.Modules
{
    public class PingCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<PingCommandHandler> _logger;

        public PingCommandHandler(DiscordSocketClient client, ILogger<PingCommandHandler> logger)
        {
            _client = client;
            _logger = logger;
        }

        [SlashCommand("ping", "Check OllimBot current latency.")]
        public async Task Ping()
        {
            var uptime = DateTimeOffset.UtcNow;
            var latency = _client.Latency;
            var status = _client.Status;


            var description =
                $"**Status**: `{status}`\n" +
                $"**Latency**: `{latency} ms`\n" +
                "**Repo**: https://github.com/Bruno0M/Ollim-Bot";

            var serverName = Context.Guild.Name;
            var serverIcon = Context.Guild.IconUrl;

            var embedBuilder = new EmbedBuilder()
                .WithAuthor(serverName, serverIcon)
                .WithColor(Color.DarkGrey)
                .WithTitle("Pong!")
                .WithDescription(description)
                .WithTimestamp(uptime)
                .Build();

            _logger.LogInformation($"{Context.User.GlobalName} pingou - {latency}");

            await RespondAsync(embed: embedBuilder);
        }
    }
}
