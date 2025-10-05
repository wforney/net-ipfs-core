namespace Ipfs.Registry;

/// <summary>
/// Tests for <see cref="Codec"/>.
/// </summary>
[TestClass]
public class CodecTest
{
    /// <summary>
    /// There is at least one registered algorithm.
    /// </summary>
    [TestMethod]
    public void Algorithms_Are_Enumerable() => Assert.AreNotEqual(0, Codec.All.Count());

    /// <summary>
    /// Bad names throw <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Bad_Name()
    {
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => Codec.Register(null!, 1));
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => Codec.Register("", 1));
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => Codec.Register("   ", 1));
    }

    /// <summary>
    /// Duplicate names or codes throw <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void Code_Already_Exists() => ExceptionAssert.Throws<ArgumentException>(() => Codec.Register("raw-x", 0x55));

    /// <summary>
    /// Duplicate names or codes throw <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void Name_Already_Exists() => ExceptionAssert.Throws<ArgumentException>(() => Codec.Register("raw", 1));

    /// <summary>
    /// Registering a new codec works.
    /// </summary>
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
