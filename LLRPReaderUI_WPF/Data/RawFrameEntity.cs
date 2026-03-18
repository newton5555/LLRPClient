using System;

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
    }
}
