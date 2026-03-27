using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LLRPReaderUI_Avalonia.Data
{
    public interface IRawFrameRepository
    {
        Task LogRawAsync(string? deviceId, string direction, byte[] payload);
        Task<List<RawFrameEntity>> GetRecentAsync(int take = 1000);
        Task<List<RawFrameEntity>> GetByFilterAsync(DateTime? startDate, DateTime? endDate, string? direction, string? deviceId, int take = 1000);
        Task<int> ClearAllAsync();
    }
}
