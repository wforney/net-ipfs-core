namespace Ipfs
{
    internal class SctpNetworkProtocol : TcpNetworkProtocol
    {
        public override string Name { get { return "sctp"; } }
        public override uint Code { get { return 132; } }
    }

}
