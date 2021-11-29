using Google.Protobuf;

namespace Ipfs
{
    internal abstract class DomainNameNetworkProtocol : NetworkProtocol
    {
        public string? DomainName { get; set; }
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            DomainName = Value;
        }
        public override void ReadValue(CodedInputStream stream)
        {
            Value = stream.ReadString();
            DomainName = Value;
        }

        public override void WriteValue(TextWriter stream)
        {
            stream.Write('/');
            stream.Write(DomainName?.ToString());
        }
        public override void WriteValue(CodedOutputStream stream)
        {
            stream.WriteString(DomainName);
        }
    }

}
