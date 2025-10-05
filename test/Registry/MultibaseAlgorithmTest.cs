namespace Ipfs.Registry;

/// <summary>
/// Tests for <see cref="MultiBaseAlgorithm"/>.
/// </summary>
[TestClass]
public class MultiBaseAlgorithmTest
{
    /// <summary>
    /// There is at least one algorithm registered.
    /// </summary>
    [TestMethod]
    public void Algorithms_Are_Enumerable() => Assert.AreNotEqual(0, MultiBaseAlgorithm.All.Count());

    /// <summary>
    /// The name cannot be null or empty.
    /// </summary>
    [TestMethod]
    public void Bad_Name()
    {
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => MultiBaseAlgorithm.Register(null!, '?'));
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => MultiBaseAlgorithm.Register("", '?'));
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => MultiBaseAlgorithm.Register("   ", '?'));
    }

    /// <summary>
    /// The code must be unique.
    /// </summary>
    [TestMethod]
    public void Code_Already_Exists() => ExceptionAssert.Throws<ArgumentException>(() => MultiBaseAlgorithm.Register("base58btc-x", 'z'));

    /// <summary>
    /// Tests for <see cref="MultiBaseAlgorithm"/> that are known but not yet implemented.
    /// </summary>
    [TestMethod]
    public void Known_But_NYI()
    {
        var alg = MultiBaseAlgorithm.Register("nyi", 'n');
        try
        {
            _ = ExceptionAssert.Throws<NotImplementedException>(() => alg.Encode(null!));
            _ = ExceptionAssert.Throws<NotImplementedException>(() => alg.Decode(null!));
        }
        finally
        {
            MultiBaseAlgorithm.Deregister(alg);
        }
    }

    /// <summary>
    /// The name must be unique.
    /// </summary>
    [TestMethod]
    public void Name_Already_Exists() => ExceptionAssert.Throws<ArgumentException>(() => MultiBaseAlgorithm.Register("base58btc", 'z'));

    /// <summary>
    /// The <see cref="MultiBaseAlgorithm.Name"/> is the same as <see cref="MultiBaseAlgorithm.ToString"/>.
    /// </summary>
    [TestMethod]
    public void Name_Is_Also_ToString()
    {
        foreach (MultiBaseAlgorithm alg in MultiBaseAlgorithm.All)
        {
            Assert.AreEqual(alg.Name, alg.ToString());
        }
    }

    /// <summary>
    /// Round trip encoding and decoding for all algorithms.
    /// </summary>
    [TestMethod]
    public void Roundtrip_All_Algorithms()
    {
        byte[] bytes = [1, 2, 3, 4, 5];

        foreach (MultiBaseAlgorithm alg in MultiBaseAlgorithm.All)
        {
            string s = alg.Encode(bytes);
            CollectionAssert.AreEqual(bytes, alg.Decode(s), alg.Name);
        }
    }
}
