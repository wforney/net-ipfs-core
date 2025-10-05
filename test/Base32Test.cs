using System.Text;

namespace Ipfs;

/// <summary>
/// Tests for <see cref="Base32"/>
/// </summary>
[TestClass]
public class Base32DecodeTests
{
    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector1() => CollectionAssert.AreEqual(GetStringBytes(string.Empty), Base32.Decode(string.Empty));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector2() => CollectionAssert.AreEqual(GetStringBytes("f"), Base32.Decode("MY======"));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector3() => CollectionAssert.AreEqual(GetStringBytes("fo"), Base32.Decode("MZXQ===="));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector4() => CollectionAssert.AreEqual(GetStringBytes("foo"), Base32.Decode("MZXW6==="));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector5() => CollectionAssert.AreEqual(GetStringBytes("foob"), Base32.Decode("MZXW6YQ="));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector6() => CollectionAssert.AreEqual(GetStringBytes("fooba"), Base32.Decode("MZXW6YTB"));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector7() => CollectionAssert.AreEqual(GetStringBytes("foobar"), Base32.Decode("MZXW6YTBOI======"));

    private static byte[] GetStringBytes(string x) => Encoding.ASCII.GetBytes(x);
}

/// <summary>
/// Tests for <see cref="Base32"/>
/// </summary>
[TestClass]
public class Base32EncodeTests
{
    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector1() => Assert.AreEqual(string.Empty, Base32.Encode(GetStringBytes(string.Empty)));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector2() => Assert.AreEqual("my", Base32.Encode(GetStringBytes("f")));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector3() => Assert.AreEqual("mzxq", Base32.Encode(GetStringBytes("fo")));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector4() => Assert.AreEqual("mzxw6", Base32.Encode(GetStringBytes("foo")));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector5() => Assert.AreEqual("mzxw6yq", Base32.Encode(GetStringBytes("foob")));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector6() => Assert.AreEqual("mzxw6ytb", Base32.Encode(GetStringBytes("fooba")));

    /// <summary>
    /// Test vectors from RFC 4648
    /// </summary>
    [TestMethod]
    public void Vector7() => Assert.AreEqual("mzxw6ytboi", Base32.Encode(GetStringBytes("foobar")));

    private static byte[] GetStringBytes(string x) => Encoding.ASCII.GetBytes(x);
}
