using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Ollim.Bot.Configurations.Handlers;
using Ollim.Domain.Repositories;
using System.Reflection;

namespace Ollim.Bot.Configurations
{
    public class OllimBot : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly DiscordExceptionHandler _exceptionHandler;
        private readonly ILogger<OllimBot> _logger;

        public static DiscordSocketClient CreateDiscordClient()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.GuildVoiceStates |
                                 GatewayIntents.MessageContent |
                                 GatewayIntents.Guilds |
                                 GatewayIntents.GuildIntegrations
            };

            return new DiscordSocketClient(config);
        }


        public OllimBot(DiscordSocketClient client, IServiceProvider serviceProvider, IConfiguration configuration, DiscordExceptionHandler exceptionHandler)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _interaction = new InteractionService(_client.Rest);
            _exceptionHandler = exceptionHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            _client.Log += Log;
            _exceptionHandler.Initialize();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\ninfo: ");
            Console.ResetColor();


            DateTime serverTime = DateTime.Now;
            TimeZoneInfo brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

            DateTime brazilTime = TimeZoneInfo.ConvertTime(serverTime, brazilTimeZone);
            Console.WriteLine($"OllimBot está iniciando em {brazilTime}\n");

            string discordToken = _configuration["Bot:DiscordToken"];

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

            _interaction.SlashCommandExecuted += async (commandInfo, context, result) =>
            {
                if (!result.IsSuccess)
                {
                    if (result is ExecuteResult execResult && execResult.Exception is not null)
                    {
                        await _exceptionHandler.SendExceptionMessageAsync(execResult.Exception, context.Guild?.Id);
                    }
                }
            };

            _client.Ready += HandleReadyAsync;
            _client.InteractionCreated += InteractionCreatedAsync;

            _client.MessageReceived += MessageReceivedHandler;

            var voiceHandler = _serviceProvider.GetRequiredService<VoiceHandler>();
            _client.UserVoiceStateUpdated += voiceHandler.UserVoiceStateUpdatedHandler;

            //await Task.Delay(-1);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var channelRepository = scope.ServiceProvider.GetRequiredService<IChannelRepository>();

            var channels = await channelRepository.GetAllNotification();
            foreach (var channel in channels)
            {
                var messageChannel = await _client.GetChannelAsync(channel.Id) as IMessageChannel;
                if (messageChannel != null)
                {
                    await messageChannel.SendMessageAsync("Ollim está desligando...");
                }
            }

            await _client.StopAsync();
            await _client.LogoutAsync();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg);

            return Task.CompletedTask;
        }

        private Task MessageReceivedHandler(SocketMessage stMessage)
        {
            Task.Run(async () =>
        {

            SocketUserMessage msg = stMessage as SocketUserMessage;
            if (msg == null) return;


            if (stMessage.Author.IsBot) return;

            var userMessage = msg.Content;

            string botMessage = string.Empty;

            List<string> messages = new List<string>()
            {
                    "dia",
                    "tarde",
                    "noite"
            };


            foreach (var message in messages)
            {
                if (userMessage.Contains($"Bom {message}", StringComparison.OrdinalIgnoreCase) && userMessage.Contains("Bot", StringComparison.OrdinalIgnoreCase))
                {
                    botMessage = $"Bom {message}, {stMessage.Author.Username}!";
                    break;
                }
            }

            var msgChannel = stMessage.Channel as IMessageChannel;

            //if (!string.IsNullOrEmpty(botMessage))
            //{
            await msgChannel.SendMessageAsync(botMessage);
            //}
        });
            return Task.CompletedTask;
        }

        private async Task HandleReadyAsync()
        {
            try
            {
                await _interaction.RegisterCommandsGloballyAsync();
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _interaction.ExecuteCommandAsync(context, _serviceProvider);
            }
            catch (Exception exception)
            {
                // Primeiro responda à interação
                if (!interaction.HasResponded)
                {
                    try
                    {
                        await interaction.RespondAsync("Ocorreu um erro ao processar o comando.", ephemeral: true);
                    }
                    catch
                    {
                        // Se falhar ao responder, tente modificar a resposta
                        try
                        {
                            await interaction.ModifyOriginalResponseAsync(props =>
                                props.Content = "Ocorreu um erro ao processar o comando.");
                        }
                        catch
                        {
                            // Se ambos falharem, apenas log
                            _logger.LogError("Não foi possível responder à interação");
                        }
                    }
                }

                // Depois envie a mensagem de erro detalhada
                ulong? guildId = interaction.GuildId;
                await _exceptionHandler.SendExceptionMessageAsync(exception, guildId);
            }
        }

    }
}
