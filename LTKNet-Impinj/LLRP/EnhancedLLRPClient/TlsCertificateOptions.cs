using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace Org.LLRP.LTK.LLRPV1
{
    /// <summary>
    /// 控制 TLS 握手期间如何验证服务端证书。
    /// <para>
    /// Impinj 读写器出厂时携带自签名证书，建议在封闭网络内使用
    /// <see cref="SkipVerification"/>，在对公网开放的场景中使用
    /// <see cref="StrictSystemValidation"/> 或 <see cref="PinThumbprint"/>。
    /// </para>
    /// </summary>
    public sealed class TlsCertificateOptions
    {
        private readonly ValidationMode mode;
        private readonly string pinnedThumbprint;

        private TlsCertificateOptions(ValidationMode mode, string pinnedThumbprint = null)
        {
            this.mode = mode;
            this.pinnedThumbprint = pinnedThumbprint;
        }

        /// <summary>
        /// 跳过证书验证，接受任意服务端证书。
        /// 适用于开发环境和携带自签名证书的读写器所在的封闭网络。
        /// </summary>
        public static TlsCertificateOptions SkipVerification { get; }
            = new TlsCertificateOptions(ValidationMode.Skip);

        /// <summary>
        /// 使用操作系统受信 CA 根证书完整验证服务端证书链。
        /// 会拒绝自签名或已过期的证书。
        /// </summary>
        public static TlsCertificateOptions StrictSystemValidation { get; }
            = new TlsCertificateOptions(ValidationMode.Strict);

        /// <summary>
        /// 仅接受 SHA-1 指纹与 <paramref name="sha1Thumbprint"/> 精确匹配的证书。
        /// 适用于已知特定自签名证书的读写器。
        /// </summary>
        /// <param name="sha1Thumbprint">
        /// 证书 SHA-1 指纹，大小写不敏感，支持带冒号或空格的格式，
        /// 例如 "A1:B2:C3…" 或 "A1B2C3…"。
        /// </param>
        /// <exception cref="ArgumentNullException">thumbprint 为空时抛出。</exception>
        public static TlsCertificateOptions PinThumbprint(string sha1Thumbprint)
        {
            if (string.IsNullOrWhiteSpace(sha1Thumbprint))
                throw new ArgumentNullException(nameof(sha1Thumbprint));

            string normalized = sha1Thumbprint
                .Replace(":", "")
                .Replace(" ", "")
                .ToUpperInvariant();

            return new TlsCertificateOptions(ValidationMode.PinnedThumbprint, normalized);
        }

        /// <summary>
        /// 根据当前策略对服务端证书执行验证，供 <see cref="System.Net.Security.RemoteCertificateValidationCallback"/> 使用。
        /// </summary>
        internal bool Validate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            switch (this.mode)
            {
                case ValidationMode.Strict:
                    return sslPolicyErrors == SslPolicyErrors.None;

                case ValidationMode.Skip:
                    return true;

                case ValidationMode.PinnedThumbprint:
                    if (certificate == null)
                        return false;
                    using (X509Certificate2 cert2 = new X509Certificate2(certificate))
                    {
                        return cert2.Thumbprint.Equals(this.pinnedThumbprint, StringComparison.OrdinalIgnoreCase);
                    }

                default:
                    return false;
            }
        }

        private enum ValidationMode
        {
            Strict,
            Skip,
            PinnedThumbprint,
        }
    }
}
