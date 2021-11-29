namespace Ipfs
{
    internal class WssNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "wss"; } }
        public override uint Code { get { return 478; } }
    }

}
