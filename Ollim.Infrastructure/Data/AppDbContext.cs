namespace Ollim.Infrastructure.Data
{
    public class AppDbContext
    {
        public Dictionary<ulong, ulong> Configuration { get; set; }

        public AppDbContext()
        {
            Configuration = new Dictionary<ulong, ulong>();
        }

        public void SetNotificationChannel(ulong guildId, ulong channelId)
        {
            Configuration[guildId] = channelId;
        }

        public ulong? GetNotificationChannel(ulong guildId)
        {
            return Configuration.ContainsKey(guildId) ? Configuration[guildId] : (ulong?)null;
        }
    }
}
