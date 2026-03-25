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

        public async Task LogRawAsync(string? deviceId, string direction, byte[] payload)
        {
            try
            {
                var entity = new RawFrameEntity
                {
                    Timestamp = DateTime.UtcNow,
                    Direction = direction,
                    Payload = payload,
                    DeviceId = deviceId
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

        public async Task<List<RawFrameEntity>> GetByFilterAsync(DateTime? startDate, DateTime? endDate, string? direction, string? deviceId, int take = 1000)
        {
            var size = take <= 0 ? 1000 : take;
            var query = _ctx.RawFrames.AsNoTracking();

            if (startDate.HasValue)
            {
                query = query.Where(f => f.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(f => f.Timestamp <= endOfDay);
            }

            // Empty or null direction/deviceId means "all" - no filtering
            if (!string.IsNullOrEmpty(direction))
            {
                query = query.Where(f => f.Direction == direction);
            }

            if (!string.IsNullOrEmpty(deviceId))
            {
                query = query.Where(f => f.DeviceId == deviceId);
            }

            return await query
                .OrderByDescending(f => f.Timestamp)
                .Take(size)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
