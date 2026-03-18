using System.Threading.Tasks;

namespace LLRPReaderUI_WPF.Data
{
    public interface IRawFrameRepository
    {
        Task LogRawAsync(string direction, byte[] payload);
    }
}
