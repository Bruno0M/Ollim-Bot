﻿using Discord;
using Discord.WebSocket;
using Ollim.Domain.Repositories;

namespace Ollim.Bot.Configurations.Handlers
{
    public class DiscordExceptionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<DiscordExceptionHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DiscordExceptionHandler(DiscordSocketClient client, ILogger<DiscordExceptionHandler> logger, IServiceProvider serviceProvider)
        {
            _client = client;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            _client.Log += LogAsync;
        }

        private async Task LogAsync(LogMessage log)
        {
            if (log.Exception is null)
                return;

            await SendExceptionMessageAsync(log.Exception);
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                _ = SendExceptionMessageAsync(exception);
            }
        }

        public async Task SendExceptionMessageAsync(Exception exception, ulong? guildId = null)
        {
            try
            {
                var baseExcpetion = exception.GetBaseException();

                using var scope = _serviceProvider.CreateScope();
                var channelRepository = scope.ServiceProvider.GetRequiredService<IChannelRepository>();

                var guild = guildId.HasValue
                    ? _client.GetGuild(guildId.Value)
                    : _client.Guilds.FirstOrDefault();

                if (guild == null)
                {
                    _logger.LogError("Guild não encontrada");
                    return;
                }

                var channel = await channelRepository.GetNotification(guild.Id);

                var messageChannel = await _client.GetChannelAsync(channel.Id) as IMessageChannel;
                if (messageChannel == null)
                {
                    _logger.LogError("Canal de erro não encontrado");
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithTitle("❌ Erro na Aplicação")
                    .WithDescription($"```\n{baseExcpetion.Message}\n```")
                    .WithColor(Color.Red)
                    .AddField("Stack Trace", $"```\n{baseExcpetion.StackTrace?.Substring(0, Math.Min(1000, baseExcpetion.StackTrace.Length))}\n```")
                    .AddField("Tipo de Exceção", baseExcpetion.GetType().FullName)
                    .WithTimestamp(DateTimeOffset.UtcNow)
                    .Build();

                await messageChannel.SendMessageAsync(embed: embed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar enviar mensagem de exceção para o Discord");
            }
        }
    }
}
