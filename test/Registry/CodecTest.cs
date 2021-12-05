using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ipfs.Registry
{
    [TestClass]
    public class CodecTest
    {
        [TestMethod]
        public void Bad_Name()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Codec.Register(null, 1));
            Assert.ThrowsException<ArgumentNullException>(() => Codec.Register("", 1));
            Assert.ThrowsException<ArgumentNullException>(() => Codec.Register("   ", 1));
        }

        [TestMethod]
        public void Name_Already_Exists()
        {
            Assert.ThrowsException<ArgumentException>(() => Codec.Register("raw", 1));
        }

        [TestMethod]
        public void Code_Already_Exists()
        {
            Assert.ThrowsException<ArgumentException>(() => Codec.Register("raw-x", 0x55));
        }

        [TestMethod]
        public void Algorithms_Are_Enumerable()
        {
            Assert.AreNotEqual(0, Codec.All.Count());
        }

        [TestMethod]
        public void Register()
        {
            var codec = Codec.Register("something-new", 0x0bad);
            try
            {
                Assert.AreEqual("something-new", codec.Name);
                Assert.AreEqual("something-new", codec.ToString());
                Assert.AreEqual(0x0bad, codec.Code);
            }
            finally
            {
                Codec.Deregister(codec);
            }
        }
    }
}
