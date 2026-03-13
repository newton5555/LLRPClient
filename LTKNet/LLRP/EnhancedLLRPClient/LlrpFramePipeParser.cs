using System;
using System.Buffers;
using System.Buffers.Binary;


namespace Org.LLRP.LTK.LLRPV1
{
    internal static class LlrpFramePipeParser
    {
        private const int HeaderSize = 10;

        public static bool TryReadFrame(
            ref ReadOnlySequence<byte> buffer,
            int maxMessageSize,
            out LlrpFrame frame)
        {
            frame = default;

            if (buffer.Length < HeaderSize)
                return false;

            Span<byte> header = stackalloc byte[HeaderSize];
            buffer.Slice(0, HeaderSize).CopyTo(header);

            ushort versionAndType = BinaryPrimitives.ReadUInt16BigEndian(header.Slice(0, 2));
            short msgType = (short)(versionAndType & 0x03FF);
            short msgVersion = (short)((versionAndType >> 10) & 0x7);
            int msgLength = BinaryPrimitives.ReadInt32BigEndian(header.Slice(2, 4));
            int msgId = BinaryPrimitives.ReadInt32BigEndian(header.Slice(6, 4));

            if (msgLength < HeaderSize)
                throw new MalformedPacket("LLRP message length is smaller than the header size.");

            if (msgLength > maxMessageSize)
                throw new MalformedPacket("LLRP message length exceeds the configured maximum size.");

            if (buffer.Length < msgLength)
                return false;

            byte[] frameData = buffer.Slice(0, msgLength).ToArray();
            buffer = buffer.Slice(msgLength);
            frame = new LlrpFrame(msgVersion, msgType, msgId, frameData);
            return true;
        }
    }
}