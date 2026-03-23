using System;
using LLRPSdk;
using Org.LLRP.LTK.LLRPV1;

namespace LLRPReaderUI_WPF.Data
{
    public class RawFrameEntity
    {

        public DateTime LocalTimestamp => Timestamp.ToLocalTime();
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        // "RX" or "TX"
        public string Direction { get; set; }
        public byte[] Payload { get; set; }
        public int? Length => Payload?.Length;

        /// <summary>
        /// 设备标识，优先使用 ReaderIdentity（MAC地址），无则使用 IP 地址
        /// </summary>
        public string? DeviceId { get; set; }

        private string? _msgTypeName;
        public string MsgTypeName
        {
            get
            {
                if (_msgTypeName == null && Payload != null && Payload.Length > 4)
                {
                    try
                    {
                        LLRPBinaryDecoder.Decode_Envelope(Payload, out var env);
                        _msgTypeName = LLRPMessageTypeNameLookup.GetTypeName(env.msg_type);
                    }
                    catch
                    {
                        _msgTypeName = "Unknown";
                    }
                }
                return _msgTypeName ?? "Unknown";
            }
        }
    }
}
