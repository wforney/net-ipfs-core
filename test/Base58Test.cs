using System.Text;

namespace Ipfs;

/// <summary>
/// Tests for <see cref="Base58"/>.
/// </summary>
[TestClass]
public class Base58Test
{
    /// <summary>
    /// C# version of base58DecodeTest in <see href="https://github.com/ipfs/java-ipfs-api/blob/master/test/org/ipfs/Test.java"/>
    /// </summary>
    [TestMethod]
    public void Decode()
    {
        Assert.AreEqual("this is a test", Encoding.UTF8.GetString(Base58.Decode("jo91waLQA1NNeBmZKUF")));
        Assert.AreEqual("this is a test", Encoding.UTF8.GetString("jo91waLQA1NNeBmZKUF".FromBase58()));
    }

    /// <summary>
    /// C# version of base58DecodeBadTest in <see href="https://github.com/ipfs/java-ipfs-api/blob/master/test/org/ipfs/Test.java"/>
    /// </summary>
    [TestMethod]
    public void Decode_Bad() => ExceptionAssert.Throws<ArgumentException>(() => Base58.Decode("jo91waLQA1NNeBmZKUF=="));

    /// <summary>
    /// C# version of base58EncodeTest in <see href="https://github.com/ipfs/java-ipfs-api/blob/master/test/org/ipfs/Test.java"/>
    /// </summary>
    [TestMethod]
    public void Encode()
    {
        Assert.AreEqual("jo91waLQA1NNeBmZKUF", Base58.Encode(Encoding.UTF8.GetBytes("this is a test")));
        Assert.AreEqual("jo91waLQA1NNeBmZKUF", Encoding.UTF8.GetBytes("this is a test").ToBase58());
    }

    /// <summary>
    /// C# version of base58Test in <see href="https://github.com/ipfs/java-ipfs-api/blob/master/test/org/ipfs/Test.java"/>
    /// </summary>
    [TestMethod]
    public void Java()
    {
        string input = "QmPZ9gcCEpqKTo6aq61g2nXGUhM4iCL3ewB6LDXZCtioEB";
        byte[] output = Base58.Decode(input);
        string encoded = Base58.Encode(output);
        Assert.AreEqual(input, encoded);
    }

    /// <summary>
    /// C# version of base58ZeroTest in <see href="https://github.com/ipfs/java-ipfs-api/blob/master/test/org/ipfs/Test.java"/>
    /// </summary>
    [TestMethod]
    public void Zero()
    {
        Assert.AreEqual("1111111", Base58.Encode(new byte[7]));
        Assert.HasCount(7, Base58.Decode("1111111"));
        Assert.IsTrue(Base58.Decode("1111111").All(b => b == 0));
    }
}
