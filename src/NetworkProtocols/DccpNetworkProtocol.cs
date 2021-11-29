namespace Ipfs
{
    internal class DccpNetworkProtocol : TcpNetworkProtocol
    {
        public override string Name { get { return "dccp"; } }
        public override uint Code { get { return 33; } }
    }

}
