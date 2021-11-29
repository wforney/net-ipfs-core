namespace Ipfs
{
    internal class DnsAddrNetworkProtocol : DomainNameNetworkProtocol
    {
        public override string Name { get { return "dnsaddr"; } }
        public override uint Code { get { return 56; } }
    }

}
