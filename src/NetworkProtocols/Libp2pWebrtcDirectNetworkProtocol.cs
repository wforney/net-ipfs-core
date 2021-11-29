namespace Ipfs
{
    internal class Libp2pWebrtcDirectNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "libp2p-webrtc-direct"; } }
        public override uint Code { get { return 276; } }
    }

}
