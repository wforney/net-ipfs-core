using Google.Protobuf;
using System.Net;
using System.Net.Sockets;

namespace Ipfs
{
    internal class Ipv6NetworkProtocol : IpNetworkProtocol
    {
        private static int AddressSize = IPAddress.IPv6Any.GetAddressBytes().Length;

        public override string Name { get { return "ip6"; } }
        public override uint Code { get { return 41; } }
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            if (Address.AddressFamily != AddressFamily.InterNetworkV6)
                throw new FormatException(string.Format("'{0}' is not a valid IPv6 address.", Value));
        }
        public override void ReadValue(CodedInputStream stream)
        {
            var a = stream.ReadSomeBytes(AddressSize);
            Address = new IPAddress(a);
            Value = Address.ToString();
        }
    }

}
