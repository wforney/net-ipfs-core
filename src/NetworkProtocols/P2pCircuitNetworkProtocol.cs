namespace Ipfs
{
    internal class P2pCircuitNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "p2p-circuit"; } }
        public override uint Code { get { return 290; } }
    }

}
