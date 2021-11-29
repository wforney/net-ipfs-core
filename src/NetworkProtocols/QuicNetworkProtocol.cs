namespace Ipfs
{
    internal class QuicNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "quic"; } }
        public override uint Code { get { return 460; } }
    }

}
