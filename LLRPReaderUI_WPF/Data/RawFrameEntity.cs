using System;
using LLRPSdk;
using Org.LLRP.LTK.LLRPV1;

namespace LLRPReaderUI_WPF.Data
{
    public class RawFrameEntity
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        // "RX" or "TX"
        public string Direction { get; set; }
        public byte[] Payload { get; set; }
        public int? Length => Payload?.Length;

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
