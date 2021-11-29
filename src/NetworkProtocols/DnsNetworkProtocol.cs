namespace Ipfs
{
    internal class DnsNetworkProtocol : DomainNameNetworkProtocol
    {
        public override string Name { get { return "dns"; } }
        public override uint Code { get { return 53; } }
    }

}
