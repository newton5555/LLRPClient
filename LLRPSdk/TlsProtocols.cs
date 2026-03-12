
#nullable disable
namespace LLRPSdk
{
  /// <summary>Enumerations for Tls protocols supported.</summary>
  public enum TlsProtocols
  {
    /// <summary>This defaults to OS selecting the protocol</summary>
    OsDefault,
    /// <summary>Use SslProtocols.Tls2</summary>
    Tls12,
    /// <summary>Use SslProtocols.Tls3</summary>
    Tls13,
  }
}
