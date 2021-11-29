namespace Ipfs
{
    internal class Libp2pWebrtcStarNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "libp2p-webrtc-star"; } }
        public override uint Code { get { return 275; } }
    }

}
