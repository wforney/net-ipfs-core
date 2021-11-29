namespace Ipfs
{
    internal class Dns6NetworkProtocol : DomainNameNetworkProtocol
    {
        public override string Name { get { return "dns6"; } }
        public override uint Code { get { return 55; } }
    }

}
