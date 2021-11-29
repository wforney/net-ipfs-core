using Google.Protobuf;

namespace Ipfs
{
    internal class P2pNetworkProtocol : NetworkProtocol
    {
        public MultiHash? MultiHash { get; private set; }
        public override string Name { get { return "p2p"; } }
        public override uint Code { get { return 421; } }
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            MultiHash = Value is null ? null : new MultiHash(Value);
        }
        public override void ReadValue(CodedInputStream stream)
        {
            stream.ReadLength();
            MultiHash = new MultiHash(stream);
            Value = MultiHash.ToBase58();
        }
        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = MultiHash?.ToArray() ?? Array.Empty<byte>();
            stream.WriteLength(bytes.Length);
            stream.WriteSomeBytes(bytes);
        }
    }

}
