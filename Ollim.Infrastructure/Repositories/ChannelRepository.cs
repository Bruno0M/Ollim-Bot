using Microsoft.EntityFrameworkCore;
using Ollim.Bot.Entities;
using Ollim.Domain.Repositories;
using Ollim.Infrastructure.Data;

namespace Ollim.Infrastructure.Repositories
{
    public class ChannelRepository : IChannelRepository
    {
        private readonly AppDbContext _context;

        public ChannelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Channel> GetNotification(ulong guildId)
        {
            return await _context.Channels
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.GuildId == guildId);
        }

        public async Task<List<Channel>> GetAllNotification()
        {
            return await _context.Channels
                .AsNoTracking()
                .ToListAsync();
        }

        public async void SetNotification(ulong guildId, ulong channelId)
        {
            var channel = new Channel()
            {
                Id = channelId,
                GuildId = guildId
            };

            await _context.Channels.AddAsync(channel);
            await _context.SaveChangesAsync();
        }
    }
}
