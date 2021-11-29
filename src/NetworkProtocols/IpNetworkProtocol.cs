using Google.Protobuf;
using System.Net;

namespace Ipfs
{
    internal abstract class IpNetworkProtocol : NetworkProtocol
    {
        public IPAddress Address { get; set; } = IPAddress.Any;
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);

            // Remove the scope id.
            int i = Value?.LastIndexOf('%') ?? -1;
            if (i != -1)
                Value = Value?[..i];

            Address = IPAddress.TryParse(Value, out var address)
                ? address
                : throw new FormatException($"'{Value}' is not a valid IP address.");
        }
        public override void WriteValue(TextWriter stream)
        {
            stream.Write('/');
            stream.Write(Address.ToString());
        }
        public override void WriteValue(CodedOutputStream stream)
        {
            var ip = Address.GetAddressBytes();
            stream.WriteSomeBytes(ip);
        }
    }

}
