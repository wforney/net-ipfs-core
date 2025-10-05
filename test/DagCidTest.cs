namespace Ipfs;

/// <summary>
/// A wrapper for a <see cref="Cid"/> that is used in an IPLD Directed Acyclic Graph (DAG).
/// </summary>
[TestClass]
public class DagCidTest
{
    /// <summary>
    /// Setting a valid CID works correctly.
    /// </summary>
    [TestMethod]
    public void Value_ValidCid_SetsSuccessfully()
    {
        // Arrange
        Cid validCid = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V";

        // Act & Assert
        var dagCid = new DagCid { Value = validCid };
        Assert.AreEqual(validCid, dagCid.Value);
    }

    /// <summary>
    /// Setting a CID with ContentType "libp2p-key" throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void Value_LibP2pKeyCid_ThrowsArgumentException()
    {
        // Arrange - using real IPNS key CID that should have libp2p-key content type
        Cid libp2pKeyCid = "k51qzi5uqu5dlvj2baxnqndepeb86cbk3ng7n3i46uzyxzyqj2xjonzllnv0v8";

        // Verify this CID actually has libp2p-key content type
        Assert.AreEqual("libp2p-key", libp2pKeyCid.ContentType);

        // Act & Assert
        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
            new DagCid { Value = libp2pKeyCid });

        Assert.Contains("Cannot store CID-encoded libp2p key as DagCid link", exception.Message);
        Assert.Contains("IPLD links must be immutable", exception.Message);
        Assert.AreEqual("value", exception.ParamName);
    }

    /// <summary>
    /// Setting a CID with ContentType "libp2p-key" after construction throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void Value_LibP2pKeyCid_SetAfterConstruction_ThrowsArgumentException()
    {
        // Arrange
        Cid validCid = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V";
        // Using another real IPNS key CID
        Cid libp2pKeyCid = "k51qzi5uqu5dlvj2baxnqndepeb86cbk3ng7n3i46uzyxzyqj2xjonzllnv0v8";

        var dagCid = new DagCid { Value = validCid };

        // Act & Assert
        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
            dagCid.Value = libp2pKeyCid);

        Assert.Contains("Cannot store CID-encoded libp2p key as DagCid link", exception.Message);
        Assert.AreEqual("value", exception.ParamName);
    }

    /// <summary>
    /// Explicit casting from a valid CID to DagCid works correctly.
    /// </summary>
    [TestMethod]
    public void ExplicitCast_ValidCid_CastsSuccessfully()
    {
        // Arrange
        Cid validCid = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V";

        // Act
        var dagCid = (DagCid)validCid;

        // Assert
        Assert.AreEqual(validCid, dagCid.Value);
    }

    /// <summary>
    /// Explicit casting from a CID with ContentType "libp2p-key" throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void ExplicitCast_LibP2pKeyCid_ThrowsArgumentException()
    {
        // Arrange - using real IPNS key CID that should have libp2p-key content type
        Cid libp2pKeyCid = "k51qzi5uqu5dlvj2baxnqndepeb86cbk3ng7n3i46uzyxzyqj2xjonzllnv0v8";

        // Act & Assert
        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
            (DagCid)libp2pKeyCid);

        Assert.Contains("Cannot cast CID-encoded libp2p key to DagCid", exception.Message);
        Assert.Contains("IPLD links must be immutable", exception.Message);
        Assert.AreEqual("cid", exception.ParamName);
    }

    /// <summary>
    /// Implicit casting from DagCid to Cid works correctly.
    /// </summary>
    [TestMethod]
    public void ImplicitCast_DagCidToCid_WorksCorrectly()
    {
        // Arrange
        Cid originalCid = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V";
        var dagCid = new DagCid { Value = originalCid };

        // Act
        Cid convertedCid = dagCid;

        // Assert
        Assert.AreEqual(originalCid, convertedCid);
    }

    /// <summary>
    /// ToString returns the string representation of the underlying Cid.
    /// </summary>
    [TestMethod]
    public void ToString_ReturnsValueToString()
    {
        // Arrange
        Cid cid = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V";
        var dagCid = new DagCid { Value = cid };

        // Act
        string result = dagCid.ToString();

        // Assert
        Assert.AreEqual(cid.ToString(), result);
    }
}
