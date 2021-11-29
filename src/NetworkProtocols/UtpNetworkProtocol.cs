namespace Ipfs
{
    internal class UtpNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "utp"; } }
        public override uint Code { get { return 302; } }
    }

}
