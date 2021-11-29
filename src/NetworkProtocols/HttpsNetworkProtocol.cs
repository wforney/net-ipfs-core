namespace Ipfs
{
    internal class HttpsNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "https"; } }
        public override uint Code { get { return 443; } }
    }

}
