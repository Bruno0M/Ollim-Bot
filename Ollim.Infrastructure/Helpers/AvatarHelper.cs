using Discord.WebSocket;

namespace Ollim.Infrastructure.Helpers
{
    public static class AvatarHelper
    {
        public static string GetAvatarIcon(SocketUser user)
        {
            string avatarUrl = user.GetAvatarUrl(size: 512) ?? user.GetDefaultAvatarUrl();
            return avatarUrl;
        }
    }
}
