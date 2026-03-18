using System;
using System.Threading.Tasks;
using LLRPReaderUI_WPF.Logging;
using Microsoft.EntityFrameworkCore;

namespace LLRPReaderUI_WPF.Data
{
    public class EfRawFrameRepository : IRawFrameRepository
    {
        private readonly RawFrameDbContext _ctx;

        public EfRawFrameRepository(RawFrameDbContext ctx)
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
            catch
            {
                // swallow errors to avoid affecting UI
            }
        }
    }
}
