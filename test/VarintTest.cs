namespace Ipfs;

/// <summary>
/// Tests for <see cref="Varint"/>.
/// </summary>
[TestClass]
public class VarintTest
{
    /// <summary>
    /// Gets or sets the test context which provides
    /// </summary>
    public TestContext TestContext { get; set; }

    /// <summary>
    /// Decode from an offset.
    /// </summary>
    [TestMethod]
    public void Decode_From_Offset()
    {
        byte[] x = [0x00, 0xAC, 0x02];
        Assert.AreEqual(300, Varint.DecodeInt32(x, 1));
    }

    /// <summary>
    /// Decode from an empty array.
    /// </summary>
    [TestMethod]
    public void Empty()
    {
        byte[] bytes = [];
        _ = ExceptionAssert.Throws<EndOfStreamException>(() => Varint.DecodeInt64(bytes));
    }

    /// <summary>
    /// Encode a negative number.
    /// </summary>
    [TestMethod]
    public void Encode_Negative() => ExceptionAssert.Throws<NotSupportedException>(() => Varint.Encode(-1));

    /// <summary>
    /// Show examples from the spec.
    /// </summary>
    [TestMethod]
    public void Example()
    {
        for (long v = 1; v <= 0xFFFFFFFL; v <<= 4)
        {
            Console.Write($"| {v} (0x{v:x}) ");
            Console.WriteLine($"| {Varint.Encode(v).ToHexString()} |");
        }
    }

    /// <summary>
    /// MaxLong
    /// </summary>
    [TestMethod]
    public void MaxLong()
    {
        byte[] x = "ffffffffffffffff7f".ToHexBuffer();
        Assert.AreEqual(9, Varint.RequiredBytes(long.MaxValue));
        CollectionAssert.AreEqual(x, Varint.Encode(long.MaxValue));
        Assert.AreEqual(long.MaxValue, Varint.DecodeInt64(x));
    }

    /// <summary>
    /// Read a varint from a stream asynchronously.
    /// </summary>
    /// <returns>The task to await.</returns>
    [TestMethod]
    public async Task ReadAsync()
    {
        using var ms = new MemoryStream("ffffffffffffffff7f".ToHexBuffer());
        long v = await ms.ReadVarint64Async(TestContext.CancellationToken);
        Assert.AreEqual(long.MaxValue, v);
    }

    /// <summary>
    /// Read a varint from a stream asynchronously but cancel the operation.
    /// </summary>
    [TestMethod]
    public void ReadAsync_Cancel()
    {
        var ms = new MemoryStream([0]);
        var cs = new CancellationTokenSource();
        cs.Cancel();
        _ = ExceptionAssert.Throws<TaskCanceledException>(() => ms.ReadVarint32Async(cs.Token).Wait(TestContext.CancellationToken));
    }

    /// <summary>
    /// Three hundred
    /// </summary>
    [TestMethod]
    public void ThreeHundred()
    {
        byte[] x = [0xAC, 0x02];
        Assert.AreEqual(2, Varint.RequiredBytes(300));
        CollectionAssert.AreEqual(x, Varint.Encode(300));
        Assert.AreEqual(300, Varint.DecodeInt32(x));
    }

    /// <summary>
    /// Too big for Int32
    /// </summary>
    [TestMethod]
    public void TooBig_Int32()
    {
        byte[] bytes = Varint.Encode((long)int.MaxValue + 1);
        _ = ExceptionAssert.Throws<InvalidDataException>(() => Varint.DecodeInt32(bytes));
    }

    /// <summary>
    /// Too big for Int64
    /// </summary>
    [TestMethod]
    public void TooBig_Int64()
    {
        byte[] bytes = "ffffffffffffffffff7f".ToHexBuffer();
        _ = ExceptionAssert.Throws<InvalidDataException>(() => Varint.DecodeInt64(bytes));
    }

    /// <summary>
    /// Unterminated varint
    /// </summary>
    [TestMethod]
    public void Unterminated()
    {
        byte[] bytes = "ff".ToHexBuffer();
        _ = ExceptionAssert.Throws<InvalidDataException>(() => Varint.DecodeInt64(bytes));
    }

    /// <summary>
    /// Write a varint to a stream asynchronously.
    /// </summary>
    /// <returns>The task to await.</returns>
    [TestMethod]
    public async Task WriteAsync()
    {
        using var ms = new MemoryStream();
        await ms.WriteVarintAsync(long.MaxValue, TestContext.CancellationToken);
        ms.Position = 0;
        Assert.AreEqual(long.MaxValue, ms.ReadVarint64());
    }

    /// <summary>
    /// Write a varint to a stream asynchronously but cancel the operation.
    /// </summary>
    [TestMethod]
    public void WriteAsync_Cancel()
    {
        var ms = new MemoryStream();
        var cs = new CancellationTokenSource();
        cs.Cancel();
        _ = ExceptionAssert.Throws<TaskCanceledException>(() => ms.WriteVarintAsync(0, cs.Token).Wait(TestContext.CancellationToken));
    }

    /// <summary>
    /// Write a negative varint to a stream asynchronously.
    /// </summary>
    [TestMethod]
    public void WriteAsync_Negative()
    {
        var ms = new MemoryStream();
        _ = ExceptionAssert.Throws<Exception>(() => ms.WriteVarintAsync(-1, TestContext.CancellationToken).Wait(TestContext.CancellationToken));
    }

    /// <summary>
    /// Zero
    /// </summary>
    [TestMethod]
    public void Zero()
    {
        byte[] x = [0];
        Assert.AreEqual(1, Varint.RequiredBytes(0));
        CollectionAssert.AreEqual(x, Varint.Encode(0));
        Assert.AreEqual(0, Varint.DecodeInt32(x));
    }
}
