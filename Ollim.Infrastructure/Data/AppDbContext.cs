using Microsoft.EntityFrameworkCore;
using Ollim.Bot.Entities;

namespace Ollim.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Channel> Channels { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Channel>(entity =>
            {
                entity.ToTable("channels", "notfications");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.GuildId).HasColumnName("guild_id");
            });

            base.OnModelCreating(builder);
        }
    }
}
