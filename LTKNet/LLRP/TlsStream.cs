

using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;


namespace Org.LLRP.LTK.LLRPV1
{
  internal class TlsStream : SslStream
  {
    private SslProtocols _sslProtocols;

    public TlsStream(
      NetworkStream innerStream,
      bool leaveInnerStreamOpen,
      RemoteCertificateValidationCallback userCertificateValidationCallback,
      SslProtocols sslProtocolsToUse)
      : base((Stream) innerStream, leaveInnerStreamOpen, userCertificateValidationCallback)
    {
      this._sslProtocols = sslProtocolsToUse;
    }

    public override SslProtocols SslProtocol => this._sslProtocols;
  }
}
