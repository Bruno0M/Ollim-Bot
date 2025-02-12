using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Ollim.Domain.Repositories;
using Ollim.Infrastructure.Data;

namespace Ollim.Bot.Configurations
{
    public class VoiceHandler
    {
        private readonly Dictionary<ulong, (DateTime? JoinedAt, DateTime? LeftAt)> userVoiceStateMap = new();
        private readonly DiscordSocketClient _client;
        private readonly ILogger<VoiceHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        public VoiceHandler(DiscordSocketClient client, ILogger<VoiceHandler> logger, IServiceProvider serviceProvider)
        {
            _client = client;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task UserVoiceStateUpdatedHandler(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            Task.Run(async () =>
            {

                var guild = after.VoiceChannel?.Guild ?? before.VoiceChannel?.Guild;
                if (guild == null) return;

                using var scoped = _serviceProvider.CreateScope();
                var _channelRepository = scoped.ServiceProvider.GetRequiredService<IChannelRepository>();

                var channel = await _channelRepository.GetNotification(guild.Id);


                var textChannel = guild.GetTextChannel(channel.Id);
                if (textChannel == null) return;


                if (before.VoiceChannel == null && after.VoiceChannel != null)
                {
                    userVoiceStateMap[user.Id] = (DateTime.UtcNow, null);
                    Console.WriteLine($"{user.Username} entrou no canal");
                    _logger.Log(LogLevel.Information, $"{user.Username} entrou no canal");
                }
                else if (before.VoiceChannel != null && after.VoiceChannel == null)
                {
                    if (userVoiceStateMap.TryGetValue(user.Id, out var entry) && entry.JoinedAt.HasValue)
                    {
                        userVoiceStateMap[user.Id] = (entry.JoinedAt, DateTime.UtcNow);

                    }
                    var embedTask = await CreateEmbed(user, entry, guild.Name, guild.IconUrl);
                    await textChannel.SendMessageAsync(embed: embedTask);
                }

            });

            return Task.CompletedTask;
        }

        private async Task<Embed> CreateEmbed(SocketUser user, (DateTime? JoinedAt, DateTime? LeftAt) entry, string serverName, string serverIcon)
        {
            if (!entry.JoinedAt.HasValue) return null; // Don't send embed if user didn't join

            var timeInChannel = DateTime.UtcNow - entry.JoinedAt.Value;
            var description = $"Parabéns <@{user.Id}> por estudar {timeInChannel.Hours:00}:{timeInChannel.Minutes:00}:{timeInChannel.Seconds:00} h";

            var embedBuilder = new EmbedBuilder()
                .WithAuthor(serverName, serverIcon)
                .WithColor(Color.DarkGrey)
                .WithTitle("Atualização de estudos!")
                .WithDescription(description)
                .Build();

            return embedBuilder;
        }

    }
}
