using Discord;
using Discord.Interactions;
using Ollim.Domain.Repositories;

namespace Ollim.Bot.Commands.Modules
{
    public class SetTextChannelCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IServiceProvider _serviceProvider;

        public SetTextChannelCommandHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [SlashCommand("set_channel", "Select the channel to send notifications to")]
        [RequireUserPermission(GuildPermission.Administrator)]

        public async Task SetTextChannelAsync(ITextChannel channel)
        {
            using var scope = _serviceProvider.CreateScope();
            var _channelRepository = scope.ServiceProvider.GetRequiredService<IChannelRepository>();

            var guild = Context.Guild;

            var channelInGuild = await _channelRepository.GetNotification(guild.Id);
            Console.WriteLine("request: " + channel.Id + " " + "inGuild: " + channelInGuild.Id);
            if (channelInGuild.Id != channel.Id)
            {
                _channelRepository.SetNotification(guild.Id, channel.Id);
                await RespondAsync($"Canal de texto configurado para: {channel.Name}");
            }

            await RespondAsync($"Esse canal já está configurado");
        }
    }
}
