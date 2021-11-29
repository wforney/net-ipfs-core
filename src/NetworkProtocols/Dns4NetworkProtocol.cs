namespace Ipfs
{
    internal class Dns4NetworkProtocol : DomainNameNetworkProtocol
    {
        public override string Name { get { return "dns4"; } }
        public override uint Code { get { return 54; } }
    }

}
