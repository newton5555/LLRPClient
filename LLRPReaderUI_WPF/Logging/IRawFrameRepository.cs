using System.Threading.Tasks;

namespace LLRPReaderUI_WPF.Logging
{
    public interface IRawFrameRepository
    {
        Task LogRawAsync(string direction, byte[] payload);
    }
}
