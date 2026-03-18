using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using LLRPReaderUI_WPF.Logging;
using Microsoft.EntityFrameworkCore;

namespace LLRPReaderUI_WPF.Data
{
    public class RawFrameRepository : IRawFrameRepository
    {
        private readonly RawFrameDbContext _ctx;

        public RawFrameRepository(RawFrameDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task LogRawAsync(string direction, byte[] payload)
        {
            try
            {
                var entity = new RawFrameEntity
                {
                    Timestamp = DateTime.UtcNow,
                    Direction = direction,
                    Payload = payload
                };

                _ctx.RawFrames.Add(entity);
                await _ctx.SaveChangesAsync().ConfigureAwait(false);
            }
            catch(Exception ee)
            {
                // swallow errors to avoid affecting UI
            }
        }

        public async Task<List<RawFrameEntity>> GetRecentAsync(int take = 1000)
        {
            var size = take <= 0 ? 1000 : take;

            return await _ctx.RawFrames
                .AsNoTracking()
                .OrderByDescending(f => f.Timestamp)
                .Take(size)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
