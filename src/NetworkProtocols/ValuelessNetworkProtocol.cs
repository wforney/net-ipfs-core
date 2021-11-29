using Google.Protobuf;

namespace Ipfs
{
    internal abstract class ValuelessNetworkProtocol : NetworkProtocol
    {
        public override void ReadValue(CodedInputStream stream)
        {
            // No value to read
        }
        public override void ReadValue(TextReader stream)
        {
            // No value to read
        }
        public override void WriteValue(CodedOutputStream stream)
        {
            // No value to write
        }
    }

}
