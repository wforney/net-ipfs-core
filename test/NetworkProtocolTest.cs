using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ipfs
{
    [TestClass]
    public class NetworkProtocolTest
    {
        [TestMethod]
        public void Stringing()
        {
            Assert.AreEqual("/tcp/8080", new MultiAddress("/tcp/8080").Protocols[0].ToString());
        }

        [TestMethod]
        public void Register_Name_Already_Exists()
        {
            Assert.ThrowsException<ArgumentException>(() => NetworkProtocol.Register<NameExists>());
        }

        [TestMethod]
        public void Register_Code_Already_Exists()
        {
            Assert.ThrowsException<ArgumentException>(() => NetworkProtocol.Register<CodeExists>());
        }

        private class NameExists : NetworkProtocol
        {
            public override string Name { get { return "tcp"; } }
            public override uint Code { get { return 0x7FFF; } }
            public override void ReadValue(CodedInputStream stream) { }
            public override void ReadValue(TextReader stream) { }
            public override void WriteValue(CodedOutputStream stream) { }
        }

        private class CodeExists : NetworkProtocol
        {
            public override string Name { get { return "x-tcp"; } }
            public override uint Code { get { return 6; } }
            public override void ReadValue(CodedInputStream stream) { }
            public override void ReadValue(TextReader stream) { }
            public override void WriteValue(CodedOutputStream stream) { }
        }

    }
}
