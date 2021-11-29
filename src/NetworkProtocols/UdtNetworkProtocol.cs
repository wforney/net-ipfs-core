namespace Ipfs
{
    internal class UdtNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name { get { return "udt"; } }
        public override uint Code { get { return 301; } }
    }

}
