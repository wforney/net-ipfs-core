namespace Ipfs;

/// <summary>
/// Tests for <see cref="HexString"/>.
/// </summary>
[TestClass]
public class HexStringTest
{
    /// <summary>
    /// Decode a hex string.
    /// </summary>
    [TestMethod]
    public void Decode()
    {
        byte[] buffer = [.. Enumerable.Range(byte.MinValue, byte.MaxValue).Select(b => (byte)b)];
        string lowerHex = string.Concat(buffer.Select(b => b.ToString("x2")).ToArray());
        string upperHex = string.Concat(buffer.Select(b => b.ToString("X2")).ToArray());

        CollectionAssert.AreEqual(buffer, lowerHex.ToHexBuffer(), "decode lower");
        CollectionAssert.AreEqual(buffer, upperHex.ToHexBuffer(), "decode upper");
    }

    /// <summary>
    /// Encode a byte array as a hex string.
    /// </summary>
    [TestMethod]
    public void Encode()
    {
        byte[] buffer = [.. Enumerable.Range(byte.MinValue, byte.MaxValue).Select(b => (byte)b)];
        string lowerHex = string.Concat(buffer.Select(b => b.ToString("x2")).ToArray());
        string upperHex = string.Concat(buffer.Select(b => b.ToString("X2")).ToArray());

        Assert.AreEqual(lowerHex, buffer.ToHexString(), "encode default");
        Assert.AreEqual(lowerHex, buffer.ToHexString("G"), "encode general");
        Assert.AreEqual(lowerHex, buffer.ToHexString("x"), "encode lower");
        Assert.AreEqual(upperHex, buffer.ToHexString("X"), "encode upper");
    }

    /// <summary>
    /// Invalid format specifier.
    /// </summary>
    [TestMethod]
    public void InvalidFormatSpecifier() => ExceptionAssert.Throws<FormatException>(() => HexString.Encode([], "..."));

    /// <summary>
    /// Invalid hex strings.
    /// </summary>
    [TestMethod]
    public void InvalidHexStrings()
    {
        _ = ExceptionAssert.Throws<InvalidDataException>(() => HexString.Decode("0"));
        _ = ExceptionAssert.Throws<InvalidDataException>(() => HexString.Decode("0Z"));
    }
}
