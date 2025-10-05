namespace Ipfs.CoreApi;

/// <summary>
/// The result from sending a <see cref="IGenericApi.PingAsync(MultiHash, int, System.Threading.CancellationToken)"/>.
/// </summary>
[TestClass]
public class PingResultTest
{
    /// <summary>
    /// Tests the properties.
    /// </summary>
    [TestMethod]
    public void Properties()
    {
        var time = TimeSpan.FromSeconds(3);
        var r = new PingResult
        {
            Success = true,
            Text = "ping",
            Time = time
        };
        Assert.IsTrue(r.Success);
        Assert.AreEqual("ping", r.Text);
        Assert.AreEqual(time, r.Time);
    }
}
