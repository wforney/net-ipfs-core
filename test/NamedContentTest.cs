namespace Ipfs;

/// <summary>
///  Tests for <see cref="NamedContent"/>.
/// </summary>
[TestClass]
public class NamedContentTest
{
    /// <summary>
    /// Tests the properties.
    /// </summary>
    [TestMethod]
    public void Properties()
    {
        var nc = new NamedContent
        {
            ContentPath = "/ipfs/...",
            NamePath = "/ipns/..."
        };
        Assert.AreEqual("/ipfs/...", nc.ContentPath);
        Assert.AreEqual("/ipns/...", nc.NamePath);
    }
}
