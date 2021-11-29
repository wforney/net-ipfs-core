namespace Ipfs
{
    internal class UdpNetworkProtocol : TcpNetworkProtocol
    {
        public override string Name { get { return "udp"; } }
        public override uint Code { get { return 273; } }
    }

}
