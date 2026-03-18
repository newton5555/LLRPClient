using System.Threading.Tasks;
using System.Collections.Generic;

namespace LLRPReaderUI_WPF.Data
{
    public interface IRawFrameRepository
    {
        Task LogRawAsync(string direction, byte[] payload);
        Task<List<RawFrameEntity>> GetRecentAsync(int take = 1000);
    }
}
