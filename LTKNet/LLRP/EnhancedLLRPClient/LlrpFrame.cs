namespace Org.LLRP.LTK.LLRPV1
{
    internal readonly struct LlrpFrame
    {
        public LlrpFrame(short version, short type, int id, byte[] data)
        {
            this.Version = version;
            this.Type = type;
            this.Id = id;
            this.Data = data;
        }

        public short Version { get; }

        public short Type { get; }

        public int Id { get; }

        public byte[] Data { get; }
    }
}