

#nullable disable
namespace LLRPSdk
{
  internal class AsyncConnectInfo
  {
    public string Address { get; set; }

    public int Port { get; set; }

    public bool UseTLS { get; set; }

    public TlsProtocols TlsProtocol { get; set; }
  }
}
