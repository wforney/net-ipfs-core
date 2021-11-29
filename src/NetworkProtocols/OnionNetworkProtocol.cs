using Google.Protobuf;
using System.Globalization;
using System.Net;

namespace Ipfs
{
    internal class OnionNetworkProtocol : NetworkProtocol
    {
        private byte[]? address;

        public byte[] Address { get => address ?? Array.Empty<byte>(); private set => address = value; }
        public ushort Port { get; private set; }
        public override string Name { get { return "onion"; } }
        public override uint Code { get { return 444; } }
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            var parts = Value?.Split(':') ?? Array.Empty<string>();
            if (parts.Length != 2)
                throw new FormatException(string.Format("'{0}' is not a valid onion address, missing the port number.", Value));
            if (parts[0].Length != 16)
                throw new FormatException(string.Format("'{0}' is not a valid onion address.", Value));
            try
            {
                Port = ushort.Parse(parts[1]);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid onion address, invalid port number.", Value), e);
            }
            if (Port < 1)
                throw new FormatException(string.Format("'{0}' is not a valid onion address, invalid port number.", Value));
            Address = parts[0].ToUpperInvariant().FromBase32();
        }
        public override void ReadValue(CodedInputStream stream)
        {
            Address = stream.ReadSomeBytes(10);
            var bytes = stream.ReadSomeBytes(2);
            Port = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
            Value = Address.ToBase32().ToLowerInvariant() + ":" + Port.ToString(CultureInfo.InvariantCulture);
        }
        public override void WriteValue(CodedOutputStream stream)
        {
            stream.WriteSomeBytes(Address);
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)Port));
            stream.WriteSomeBytes(bytes);
        }
    }

}
