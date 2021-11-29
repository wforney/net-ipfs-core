using Google.Protobuf;
using System.Net;
using System.Net.Sockets;

namespace Ipfs
{
    internal class Ipv4NetworkProtocol : IpNetworkProtocol
    {
        private static int AddressSize = IPAddress.Any.GetAddressBytes().Length;

        public override string Name { get { return "ip4"; } }
        public override uint Code { get { return 4; } }
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            if (Address.AddressFamily != AddressFamily.InterNetwork)
                throw new FormatException(string.Format("'{0}' is not a valid IPv4 address.", Value));
        }
        public override void ReadValue(CodedInputStream stream)
        {
            var a = stream.ReadSomeBytes(AddressSize);
            Address = new IPAddress(a);
            Value = Address.ToString();
        }

    }

}
