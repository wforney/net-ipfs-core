namespace Ipfs.Registry;

/// <summary>
/// Tests the <see cref="HashingAlgorithm"/> class.
/// </summary>
[TestClass]
public class HashingAlgorithmTest
{
    /// <summary>
    /// Gets a hasher by name.
    /// </summary>
    [TestMethod]
    public void GetHasher()
    {
        using System.Security.Cryptography.HashAlgorithm hasher = HashingAlgorithm.GetAlgorithm("sha3-256");
        Assert.IsNotNull(hasher);
        byte[] input = new byte[] { 0xe9 };
        byte[] expected = "f0d04dd1e6cfc29a4460d521796852f25d9ef8d28b44ee91ff5b759d72c1e6d6".ToHexBuffer();

        byte[] actual = hasher.ComputeHash(input);
        CollectionAssert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the double-SHA256 implementation can be reused.
    /// </summary>
    [TestMethod]
    public void DoubleSha256Hasher_CanBeReused()
    {
        using System.Security.Cryptography.HashAlgorithm hasher = HashingAlgorithm.GetAlgorithm("dbl-sha2-256");
        byte[] input = [0xe9];
        byte[] expected = System.Security.Cryptography.SHA256.HashData(
            System.Security.Cryptography.SHA256.HashData(input));

        CollectionAssert.AreEqual(expected, hasher.ComputeHash(input));
        CollectionAssert.AreEqual(expected, hasher.ComputeHash(input));
    }

    /// <summary>
    /// Gets a hasher by alias.
    /// </summary>
    [TestMethod]
    public void GetHasher_Unknown() => ExceptionAssert.Throws<KeyNotFoundException>(() => HashingAlgorithm.GetAlgorithm("unknown"));

    /// <summary>
    /// Gets the metadata for a hashing algorithm.
    /// </summary>
    [TestMethod]
    public void GetMetadata()
    {
        var info = HashingAlgorithm.GetAlgorithmMetadata("sha3-256");
        Assert.IsNotNull(info);
        Assert.AreEqual("sha3-256", info.Name);
        Assert.AreEqual(0x16, info.Code);
        Assert.AreEqual(256 / 8, info.DigestSize);
        Assert.IsNotNull(info.Hasher);
    }

    /// <summary>
    /// Gets the metadata for a hashing algorithm by alias.
    /// </summary>
    [TestMethod]
    public void GetMetadata_Alias()
    {
        var info = HashingAlgorithm.GetAlgorithmMetadata("id");
        Assert.IsNotNull(info);
        Assert.AreEqual("identity", info.Name);
        Assert.AreEqual(0, info.Code);
        Assert.AreEqual(0, info.DigestSize);
        Assert.IsNotNull(info.Hasher);
    }

    /// <summary>
    /// Gets the metadata for an unknown hashing algorithm.
    /// </summary>
    [TestMethod]
    public void GetMetadata_Unknown() => ExceptionAssert.Throws<KeyNotFoundException>(() => HashingAlgorithm.GetAlgorithmMetadata("unknown"));

    /// <summary>
    /// Registers an alias that already exists.
    /// </summary>
    [TestMethod]
    public void HashingAlgorithm_Alias_Already_Exists() => ExceptionAssert.Throws<ArgumentException>(() => HashingAlgorithm.RegisterAlias("id", "identity"));

    /// <summary>
    /// Registers an alias to a hashing algorithm that does not exist.
    /// </summary>
    [TestMethod]
    public void HashingAlgorithm_Alias_Target_Does_Not_Exist() => ExceptionAssert.Throws<ArgumentException>(() => HashingAlgorithm.RegisterAlias("foo", "sha1-x"));

    /// <summary>
    /// Registers an alias with a bad target name.
    /// </summary>
    [TestMethod]
    public void HashingAlgorithm_Alias_Target_Is_Bad() => ExceptionAssert.Throws<ArgumentException>(() => HashingAlgorithm.RegisterAlias("foo", "  "));

    /// <summary>
    /// Registers an alias with a bad alias name.
    /// </summary>
    [TestMethod]
    public void HashingAlgorithm_Bad_Alias()
    {
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => HashingAlgorithm.RegisterAlias(null!, "sha1"));
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => HashingAlgorithm.RegisterAlias("", "sha1"));
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => HashingAlgorithm.RegisterAlias("   ", "sha1"));
    }

    /// <summary>
    /// Registers a new hashing algorithm.
    /// </summary>
    [TestMethod]
    public void HashingAlgorithm_Bad_Name()
    {
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => HashingAlgorithm.Register(null!, 1, 1));
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => HashingAlgorithm.Register("", 1, 1));
        _ = ExceptionAssert.Throws<ArgumentNullException>(() => HashingAlgorithm.Register("   ", 1, 1));
    }

    /// <summary>
    /// Registers a new hashing algorithm with a name or number that already exists.
    /// </summary>
    [TestMethod]
    public void HashingAlgorithm_Name_Already_Exists() => ExceptionAssert.Throws<ArgumentException>(() => HashingAlgorithm.Register("sha1", 0x11, 1));

    /// <summary>
    /// Registers a new hashing algorithm with a name or number that already exists.
    /// </summary>
    [TestMethod]
    public void HashingAlgorithm_Number_Already_Exists() => ExceptionAssert.Throws<ArgumentException>(() => HashingAlgorithm.Register("sha1-x", 0x11, 1));

    /// <summary>
    /// Hashing algorithms are enumerable.
    /// </summary>
    [TestMethod]
    public void HashingAlgorithms_Are_Enumerable() => Assert.IsLessThanOrEqualTo(HashingAlgorithm.All.Count(), 5);

    /// <summary>
    /// Verifies that removing an alias leaves the canonical algorithm registered.
    /// </summary>
    [TestMethod]
    public void DeregisteringAlias_PreservesCanonicalAlgorithm()
    {
        string aliasName = "test-alias-" + Guid.NewGuid().ToString("N");
        HashingAlgorithm alias = HashingAlgorithm.RegisterAlias(aliasName, "sha3-256");

        HashingAlgorithm.Deregister(alias);

        Assert.AreEqual("sha3-256", HashingAlgorithm.GetAlgorithmMetadata("sha3-256").Name);
        _ = ExceptionAssert.Throws<KeyNotFoundException>(() => HashingAlgorithm.GetAlgorithmMetadata(aliasName));
    }

    /// <summary>
    /// Verifies that concurrent registrations can be looked up safely.
    /// </summary>
    [TestMethod]
    public void ConcurrentRegistrationAndLookup()
    {
        const int count = 32;
        string prefix = "test-concurrent-" + Guid.NewGuid().ToString("N");
        HashingAlgorithm[] algorithms = new HashingAlgorithm[count];

        Parallel.For(0, count, i =>
        {
            algorithms[i] = HashingAlgorithm.Register($"{prefix}-{i}", 0x70000000 + i, 32, System.Security.Cryptography.SHA256.Create);
        });

        Parallel.For(0, count, i =>
        {
            Assert.AreEqual($"{prefix}-{i}", HashingAlgorithm.GetAlgorithmMetadata($"{prefix}-{i}").Name);
        });

        Parallel.ForEach(algorithms, HashingAlgorithm.Deregister);
    }
}
