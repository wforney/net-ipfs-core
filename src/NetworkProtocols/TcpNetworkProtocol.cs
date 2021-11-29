using Google.Protobuf;
using System.Globalization;
using System.Net;

namespace Ipfs
{
    internal class TcpNetworkProtocol : NetworkProtocol
    {
        public ushort Port { get; set; }
        public override string Name { get { return "tcp"; } }
        public override uint Code { get { return 6; } }
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            Port = ushort.TryParse(Value, out var port)
                ? port
                : throw new FormatException($"'{Value}' is not a valid port number.");
        }

        public override void ReadValue(CodedInputStream stream)
        {
            var bytes = stream.ReadSomeBytes(2);
            Port = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
            Value = Port.ToString(CultureInfo.InvariantCulture);
        }
        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)Port));
            stream.WriteSomeBytes(bytes);
        }
    }

}
