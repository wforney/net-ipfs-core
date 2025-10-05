namespace Ipfs;

/// <summary>
/// Unit tests for <see cref="DagLink"/>.
/// </summary>
[TestClass]
public class DagLinkTest
{
    /// <summary>
    /// Test vector from
    /// </summary>
    [TestMethod]
    public void Cid_V1()
    {
        var link = new DagLink("hello", "zB7NCdng5WffuNCgHu4PhDj7nbtuVrhPc2pMhanNxYKRsECdjX9nd44g6CRu2xNrj2bG2NNaTsveL5zDGWhbfiug3VekW", 11);
        Assert.AreEqual("hello", link.Name);
        Assert.AreEqual(1, link.Id.Version);
        Assert.AreEqual("raw", link.Id.ContentType);
        Assert.AreEqual("sha2-512", link.Id.Hash.Algorithm.Name);
        Assert.AreEqual<ulong>(11, link.Size);
    }

    /// <summary>
    /// Test cloning a link.
    /// </summary>
    [TestMethod]
    public void Cloning()
    {
        var link = new DagLink("abc", "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V", 5);
        var clone = new DagLink(link);

        Assert.AreEqual("abc", clone.Name);
        Assert.AreEqual("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V", (string)clone.Id);
        Assert.AreEqual<ulong>(5, clone.Size);
    }

    /// <summary>
    /// Test creating a link.
    /// </summary>
    [TestMethod]
    public void Creating()
    {
        var link = new DagLink("abc", "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V", 5);
        Assert.AreEqual("abc", link.Name);
        Assert.AreEqual("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V", (string)link.Id);
        Assert.AreEqual<ulong>(5, link.Size);
    }

    /// <summary>
    /// Test encoding a link.
    /// </summary>
    [TestMethod]
    public void Encoding()
    {
        string encoded = "0a22122023dca2a7429612378554b0bb5b85012dec00a17cc2c673f17d2b76a50b839cd51201611803";
        var link = new DagLink("a", "QmQke7LGtfu3GjFP3AnrP8vpEepQ6C5aJSALKAq653bkRi", 3);
        Assert.AreEqual(encoded, link.ToArray().ToHexString());
    }

    /// <summary>
    /// Test encoding a link with an empty name.
    /// </summary>
    [TestMethod]
    public void Encoding_EmptyName()
    {
        string encoded = "0a22122023dca2a7429612378554b0bb5b85012dec00a17cc2c673f17d2b76a50b839cd512001803";
        var link = new DagLink("", "QmQke7LGtfu3GjFP3AnrP8vpEepQ6C5aJSALKAq653bkRi", 3);
        Assert.AreEqual(encoded, link.ToArray().ToHexString());
    }

    /// <summary>
    /// Test encoding a link with a null name.
    /// </summary>
    [TestMethod]
    public void Encoding_NullName()
    {
        string encoded = "0a22122023dca2a7429612378554b0bb5b85012dec00a17cc2c673f17d2b76a50b839cd51803";
        var link = new DagLink(null, "QmQke7LGtfu3GjFP3AnrP8vpEepQ6C5aJSALKAq653bkRi", 3);
        Assert.AreEqual(encoded, link.ToArray().ToHexString());
    }
}
