namespace Ipfs
{
    internal class WsNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "ws"; } }
        public override uint Code { get { return 477; } }
    }

}
