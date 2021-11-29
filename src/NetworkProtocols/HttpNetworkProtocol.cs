namespace Ipfs
{
    internal class HttpNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "http"; } }
        public override uint Code { get { return 480; } }
    }

}
