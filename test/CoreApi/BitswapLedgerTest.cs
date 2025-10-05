namespace Ipfs.CoreApi;

/// <summary>
/// Tests for <see cref="BitswapLedger"/>.
/// </summary>
[TestClass]
public class BitswapLedgerTest
{
    /// <summary>
    /// Tests the <see cref="BitswapLedger.DebtRatio"/> and <see cref="BitswapLedger.IsInDebt"/>
    /// </summary>
    [TestMethod]
    public void DebtRatio_Negative()
    {
        var ledger = new BitswapLedger { DataReceived = 1024 * 1024 };
        Assert.IsTrue(ledger.DebtRatio < 1);
        Assert.IsTrue(ledger.IsInDebt);
    }

    /// <summary>
    /// Tests the <see cref="BitswapLedger.DebtRatio"/> and <see cref="BitswapLedger.IsInDebt"/>
    /// </summary>
    [TestMethod]
    public void DebtRatio_Positive()
    {
        var ledger = new BitswapLedger { DataSent = 1024 * 1024 };
        Assert.IsTrue(ledger.DebtRatio >= 1);
        Assert.IsFalse(ledger.IsInDebt);
    }

    /// <summary>
    /// Tests the default values of a <see cref="BitswapLedger"/>.
    /// </summary>
    [TestMethod]
    public void Defaults()
    {
        var ledger = new BitswapLedger();
        Assert.IsNull(ledger.Peer);
        Assert.AreEqual(0ul, ledger.BlocksExchanged);
        Assert.AreEqual(0ul, ledger.DataReceived);
        Assert.AreEqual(0ul, ledger.DataSent);
        Assert.AreEqual(0f, ledger.DebtRatio);
        Assert.IsTrue(ledger.IsInDebt);
    }
}
