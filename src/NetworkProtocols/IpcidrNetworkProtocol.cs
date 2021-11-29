using Google.Protobuf;
using System.Globalization;
using System.Net;

namespace Ipfs
{
    internal class IpcidrNetworkProtocol : NetworkProtocol
    {
        public ushort RoutingPrefix { get; set; }
        public override string Name { get { return "ipcidr"; } }
        /// <summary>
        /// The IPFS numeric code assigned to the network protocol.
        /// </summary>
        /// <value>The code.</value>
        /// <remarks>
        /// TODO: https://github.com/multiformats/multiaddr/issues/60
        /// </remarks>
        public override uint Code { get { return 999; } }
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            RoutingPrefix = ushort.TryParse(Value, out var value)
                ? value
                : throw new FormatException($"'{Value}' is not a valid routing prefix.");
        }
        public override void ReadValue(CodedInputStream stream)
        {
            var bytes = stream.ReadSomeBytes(2);
            RoutingPrefix = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
            Value = RoutingPrefix.ToString(CultureInfo.InvariantCulture);
        }
        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)RoutingPrefix));
            stream.WriteSomeBytes(bytes);
        }
    }

}
