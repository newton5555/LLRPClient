using Microsoft.EntityFrameworkCore;

namespace LLRPReaderUI_WPF.Logging
{
    public class RawFrameDbContext : DbContext
    {
        public RawFrameDbContext(DbContextOptions<RawFrameDbContext> options) : base(options)
        {
        }

        public DbSet<RawFrameEntity> RawFrames { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RawFrameEntity>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Timestamp).IsRequired();
                eb.Property(e => e.Direction).HasMaxLength(4).IsRequired();
                eb.Property(e => e.Payload).IsRequired();
            });
        }
    }
}
