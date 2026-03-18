using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LLRPReaderUI_WPF.Logging
{
    public static class RawFrameRepository
    {
        private static DbContextOptions<RawFrameDbContext>? _options;

        public static void Init(string dbPath)
        {
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var builder = new DbContextOptionsBuilder<RawFrameDbContext>();
            builder.UseSqlite($"Data Source={dbPath}");
            _options = builder.Options;

            using var ctx = new RawFrameDbContext(_options);
            ctx.Database.EnsureCreated();
        }

        public static async Task LogRawAsync(string direction, byte[] payload)
        {
            if (_options == null)
                return;

            try
            {
                var entity = new RawFrameEntity
                {
                    Timestamp = DateTime.UtcNow,
                    Direction = direction,
                    Payload = payload
                };

                using var ctx = new RawFrameDbContext(_options);
                ctx.RawFrames.Add(entity);
                await ctx.SaveChangesAsync().ConfigureAwait(false);
            }
            catch
            {
                // swallow to avoid impacting UI/reader
            }
        }

        public static void LogRaw(string direction, byte[] payload)
        {
            _ = LogRawAsync(direction, payload);
        }
    }
}
