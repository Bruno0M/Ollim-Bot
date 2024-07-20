using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Ollim.Bot.Services;
using System.Reflection;

namespace Ollim.Bot.Configurations
{
    public class OllimBot : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public OllimBot(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildPresences | 
                                 GatewayIntents.GuildVoiceStates | GatewayIntents.Guilds | 
                                 GatewayIntents.GuildIntegrations | GatewayIntents.GuildMessages
            };

            _client = new DiscordSocketClient(config);
            _interaction = new InteractionService(_client.Rest);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            _client.Log += Log;

            string discordToken = _configuration.GetSection("Bot:DiscordToken").Value ?? throw new Exception("Missing Discord token");

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

            _client.Ready += HandleReadyAsync;
            _client.InteractionCreated += InteractionCreatedAsync;

            var voiceHandler = _serviceProvider.GetRequiredService<VoiceHandler>();
            _client.UserVoiceStateUpdated += voiceHandler.UserVoiceStateUpdatedHandler;

            await Task.Delay(-1);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _client.LogoutAsync();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        private Task TestGetGuild(ulong guild)
        {
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
            var context = new SocketInteractionContext(_client, interaction);
            await _interaction.ExecuteCommandAsync(context, _serviceProvider);
        }

    }
}
