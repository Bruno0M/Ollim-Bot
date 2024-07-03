using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Ollim.Infrastructure.Helpers;
using Ollim.Infrastructure.Interfaces;

namespace Ollim.Bot.Commands.Modules
{
    public class ImageCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {

        private readonly IImageProfileProcessingService _imageProfileProcessingService;

        public ImageCommandHandler(IImageProfileProcessingService imageProfileProcessingService)
        {
            _imageProfileProcessingService = imageProfileProcessingService;
        }

        [SlashCommand("me", "Your profile")]
        public async Task Profile()
        {
           SocketUser user = Context.User;

            if (user == null)
            {
                await RespondAsync("Unable to get user information.");
                return;
            }

            string avatarUrl = AvatarHelper.GetAvatarIcon(user);
            string username = user.Username;

            string fileName = "profile.png";
            string tempPath = Path.GetTempPath();
            string filePath = Path.Combine(tempPath, fileName);

            using (var httpClient = new HttpClient())
            {

                var avatarBytes = await httpClient.GetByteArrayAsync(avatarUrl);


                string processedFilePath = Path.Combine(tempPath, "processed_" + fileName);

                _imageProfileProcessingService.ConstructImageProfile(avatarBytes, username, processedFilePath);

                await Context.Channel.SendFileAsync(processedFilePath);

                var embed = new EmbedBuilder()
                    .WithTitle("Image Upload")
                    .WithDescription($"File name: {fileName}")
                    .WithImageUrl($"attachment://processed_{fileName}")
                    .Build();

                await RespondAsync(embed: embed);
            }
        }
    }
}
