using System.Text;
using Ipfs.Registry;
using Newtonsoft.Json;

namespace Ipfs;

/// <summary>
/// Tests the <see cref="MultiHash"/> class.
/// </summary>
[TestClass]
public class MultiHashTest
{
    /// <summary>
    /// Tests known hash names.
    /// </summary>
    [TestMethod]
    public void HashNames()
    {
        _ = new MultiHash("sha1", new byte[20]);
        _ = new MultiHash("sha2-256", new byte[32]);
        _ = new MultiHash("sha2-512", new byte[64]);
        _ = new MultiHash("keccak-512", new byte[64]);
    }

    /// <summary>
    /// Tests unknown hash names.
    /// </summary>
    [TestMethod]
    public void Unknown_Hash_Name()
    {
        _ = ExceptionAssert.Throws<ArgumentException, MultiHash>(() => new MultiHash(string.Empty, []));
        _ = ExceptionAssert.Throws<ArgumentException, MultiHash>(() => new MultiHash("md5", []));
    }

    /// <summary>
    /// Tests parsing a MultiHash with an unknown hash number.
    /// </summary>
    [TestMethod]
    public void Parsing_Unknown_Hash_Number()
    {
        HashingAlgorithm? unknown = null;
        void unknownHandler(object? s, UnknownHashingAlgorithmEventArgs e) => unknown = e.Algorithm;
        var ms = new MemoryStream([0x01, 0x02, 0x0a, 0x0b]);
        MultiHash.UnknownHashingAlgorithm += unknownHandler;
        try
        {
            var mh = new MultiHash(ms);
            Assert.AreEqual("ipfs-1", mh.Algorithm.Name);
            Assert.AreEqual("ipfs-1", mh.Algorithm.ToString());
            Assert.AreEqual(1, mh.Algorithm.Code);
            Assert.AreEqual(2, mh.Algorithm.DigestSize);
            Assert.AreEqual(0xa, mh.Digest[0]);
            Assert.AreEqual(0xb, mh.Digest[1]);
            Assert.IsNotNull(unknown, "unknown handler not called");
            Assert.AreEqual("ipfs-1", unknown!.Name);
            Assert.AreEqual(1, unknown.Code);
            Assert.AreEqual(2, unknown.DigestSize);
        }
        finally
        {
            MultiHash.UnknownHashingAlgorithm -= unknownHandler;
        }
    }

    /// <summary>
    /// Tests parsing a MultiHash with a wrong digest size.
    /// </summary>
    [TestMethod]
    public void Parsing_Wrong_Digest_Size()
    {
        var ms = new MemoryStream([0x11, 0x02, 0x0a, 0x0b]);
        _ = ExceptionAssert.Throws<InvalidDataException, MultiHash>(() => new MultiHash(ms));
    }

    /// <summary>
    /// Tests invalid digest sizes.
    /// </summary>
    [TestMethod]
    public void Invalid_Digest()
    {
        _ = ExceptionAssert.Throws<ArgumentException, MultiHash>(() => new MultiHash("sha1", []));
        _ = ExceptionAssert.Throws<ArgumentException, MultiHash>(() => new MultiHash("sha1", new byte[21]));
    }

    /// <summary>
    /// Tests Base58 encoding and decoding.
    /// </summary>
    [TestMethod]
    public void Base58_Encode_Decode()
    {
        var mh = new MultiHash("QmPZ9gcCEpqKTo6aq61g2nXGUhM4iCL3ewB6LDXZCtioEB");
        Assert.AreEqual("sha2-256", mh.Algorithm.Name);
        Assert.HasCount(32, mh.Digest);
        Assert.AreEqual("QmPZ9gcCEpqKTo6aq61g2nXGUhM4iCL3ewB6LDXZCtioEB", mh.ToBase58());
    }

    /// <summary>
    /// Tests Base32 encoding.
    /// </summary>
    [TestMethod]
    public void Base32_Encode()
    {
        var mh = new MultiHash("QmPZ9gcCEpqKTo6aq61g2nXGUhM4iCL3ewB6LDXZCtioEB");
        Assert.AreEqual("sha2-256", mh.Algorithm.Name);
        Assert.HasCount(32, mh.Digest);
        Assert.AreEqual("ciqbed3k6ya5i3qqwljochwxdrk5exzqilbckapedujenz5b5hj5r3a", mh.ToBase32());
    }

    /// <summary>
    /// Computes a hash from a byte array.
    /// </summary>
    [TestMethod]
    public void Compute_Hash_Array()
    {
        byte[] hello = Encoding.UTF8.GetBytes("Hello, world.");
        var mh = MultiHash.ComputeHash(hello);
        Assert.AreEqual(MultiHash.DefaultAlgorithmName, mh.Algorithm.Name);
        Assert.IsNotNull(mh.Digest);
    }

    /// <summary>
    /// Computes a hash from a stream.
    /// </summary>
    [TestMethod]
    public void Compute_Hash_Stream()
    {
        var hello = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world."))
        {
            Position = 0
        };
        var mh = MultiHash.ComputeHash(hello);
        Assert.AreEqual(MultiHash.DefaultAlgorithmName, mh.Algorithm.Name);
        Assert.IsNotNull(mh.Digest);
    }

    /// <summary>
    /// Tries to compute a hash with an unknown algorithm.
    /// </summary>
    [TestMethod]
    public void Compute_Not_Implemented_Hash_Array()
    {
        var alg = HashingAlgorithm.Register("not-implemented", 0x0F, 32);
        try
        {
            byte[] hello = Encoding.UTF8.GetBytes("Hello, world.");
            _ = ExceptionAssert.Throws<NotImplementedException>(() => MultiHash.ComputeHash(hello, "not-implemented"));
        }
        finally
        {
            HashingAlgorithm.Deregister(alg);
        }
    }

    /// <summary>
    /// Tests the <see cref="MultiHash.Matches(byte[])"/> and <see cref="MultiHash.Matches(Stream)"/> methods.
    /// </summary>
    [TestMethod]
    public void Matches_Array()
    {
        byte[] hello = Encoding.UTF8.GetBytes("Hello, world.");
        byte[] hello1 = Encoding.UTF8.GetBytes("Hello, world");
        var mh = MultiHash.ComputeHash(hello);
        Assert.IsTrue(mh.Matches(hello));
        Assert.IsFalse(mh.Matches(hello1));

        var mh1 = MultiHash.ComputeHash(hello, "sha1");
        Assert.IsTrue(mh1.Matches(hello));
        Assert.IsFalse(mh1.Matches(hello1));

        var mh2 = MultiHash.ComputeHash(hello, "sha2-512");
        Assert.IsTrue(mh2.Matches(hello));
        Assert.IsFalse(mh2.Matches(hello1));

        var mh3 = MultiHash.ComputeHash(hello, "keccak-512");
        Assert.IsTrue(mh3.Matches(hello));
        Assert.IsFalse(mh3.Matches(hello1));
    }

    /// <summary>
    /// Tests the <see cref="MultiHash.Matches(Stream)"/> method.
    /// </summary>
    [TestMethod]
    public void Matches_Stream()
    {
        var hello = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world."));
        var hello1 = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world"));
        hello.Position = 0;
        var mh = MultiHash.ComputeHash(hello);

        hello.Position = 0;
        Assert.IsTrue(mh.Matches(hello));

        hello1.Position = 0;
        Assert.IsFalse(mh.Matches(hello1));
    }

    /// <summary>
    /// Tests known MultiHash formats.
    /// </summary>
    [TestMethod]
    public void Wire_Formats()
    {
        string[] hashes =
        [
            "5drNu81uhrFLRiS4bxWgAkpydaLUPW", // sha1
            "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4", // sha2_256
            "8Vtkv2tdQ43bNGdWN9vNx9GVS9wrbXHk4ZW8kmucPmaYJwwedXir52kti9wJhcik4HehyqgLrQ1hBuirviLhxgRBNv", // sha2_512
        ];
        byte[] helloWorld = Encoding.UTF8.GetBytes("hello world");
        foreach (string? hash in hashes)
        {
            var mh = new MultiHash(hash);
            Assert.IsTrue(mh.Matches(helloWorld), hash);
        }
    }

    /// <summary>
    /// Tests that <see cref="MultiHash.ToString()"/> is the Base58 representation.
    /// </summary>
    [TestMethod]
    public void To_String_Is_Base58_Representation()
    {
        string hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        var mh = new MultiHash(hash);
        Assert.AreEqual(hash, mh.ToString());
    }

    /// <summary>
    /// Tests implicit conversion from <see cref="string"/> to <see cref="MultiHash"/>.
    /// </summary>
    [TestMethod]
    public void Implicit_Conversion_From_String()
    {
        string hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        MultiHash mh = hash;
        Assert.IsNotNull(mh);
        Assert.IsInstanceOfType(mh, typeof(MultiHash));
        Assert.AreEqual(hash, mh.ToString());
    }

    /// <summary>
    /// Tests value equality.
    /// </summary>
    [TestMethod]
    public void Value_Equality()
    {
        var a0 = new MultiHash("QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4");
        var a1 = new MultiHash("QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4");
        var b = new MultiHash("QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L5");
        MultiHash? c = null;
        MultiHash? d = null;

        Assert.IsTrue(c == d);
        Assert.IsFalse(c == b);
        Assert.IsFalse(b == c);

        Assert.IsFalse(c != d);
        Assert.IsTrue(c != b);
        Assert.IsTrue(b != c);

#pragma warning disable 1718
        Assert.IsTrue(a0 == a0);
        Assert.IsTrue(a0 == a1);
        Assert.IsFalse(a0 == b);

#pragma warning disable 1718
        Assert.IsFalse(a0 != a0);
        Assert.IsFalse(a0 != a1);
        Assert.IsTrue(a0 != b);

        Assert.IsTrue(a0.Equals(a0));
        Assert.IsTrue(a0.Equals(a1));
        Assert.IsFalse(a0.Equals(b));

        Assert.AreEqual(a0, a0);
        Assert.AreEqual(a0, a1);
        Assert.AreNotEqual(a0, b);

        Assert.AreEqual<MultiHash>(a0, a0);
        Assert.AreEqual<MultiHash>(a0, a1);
        Assert.AreNotEqual<MultiHash>(a0, b);

        Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
        Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
        Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    /// Tests varint encoding of the hash code and length.
    /// </summary>
    [TestMethod]
    public void Varint_Hash_Code_and_Length()
    {
        byte[] concise = "1220f8c3bf62a9aa3e6fc1619c250e48abe7519373d3edf41be62eb5dc45199af2ef"
            .ToHexBuffer();
        var mh = new MultiHash(new MemoryStream(concise, false));
        Assert.AreEqual("sha2-256", mh.Algorithm.Name);
        Assert.AreEqual(0x12, mh.Algorithm.Code);
        Assert.AreEqual(0x20, mh.Algorithm.DigestSize);

        byte[] longer = "9200a000f8c3bf62a9aa3e6fc1619c250e48abe7519373d3edf41be62eb5dc45199af2ef"
            .ToHexBuffer();
        mh = new MultiHash(new MemoryStream(longer, false));
        Assert.AreEqual("sha2-256", mh.Algorithm.Name);
        Assert.AreEqual(0x12, mh.Algorithm.Code);
        Assert.AreEqual(0x20, mh.Algorithm.DigestSize);
    }

    /// <summary>
    /// Computes a hash using all known algorithms.
    /// </summary>
    [TestMethod]
    public void Compute_Hash_All_Algorithms()
    {
        foreach (HashingAlgorithm alg in HashingAlgorithm.All)
        {
            try
            {
                var mh = MultiHash.ComputeHash([], alg.Name);
                Assert.IsNotNull(mh, alg.Name);
                Assert.AreEqual(alg.Code, mh.Algorithm.Code, alg.Name);
                Assert.AreEqual(alg.Name, mh.Algorithm.Name, alg.Name);
                Assert.AreEqual(alg.DigestSize, mh.Algorithm.DigestSize, alg.Name);
                Assert.HasCount(alg.DigestSize, mh.Digest, alg.Name);
            }
            catch (NotImplementedException)
            {
                // If NYI then can't test it.
            }
        }
    }

    /// <summary>
    /// An example of computing a MultiHash.
    /// </summary>
    [TestMethod]
    public void Example()
    {
        byte[] hello = Encoding.UTF8.GetBytes("Hello world");
        var mh = MultiHash.ComputeHash(hello, "sha2-256");
        Console.WriteLine($"| hash code | 0x{mh.Algorithm.Code:x} |");
        Console.WriteLine($"| digest length | 0x{mh.Digest.Length:x} |");
        Console.WriteLine($"| digest value | {mh.Digest.ToHexString()} |");
        Console.WriteLine($"| binary | {mh.ToArray().ToHexString()} |");
        Console.WriteLine($"| base 58 | {mh.ToBase58()} |");
        Console.WriteLine($"| base 32 | {mh.ToBase32()} |");
    }

    private class TestVector
    {
        public string? Algorithm { get; set; }
        public string? Input { get; set; }
        public string? Output { get; set; }
        public bool Ignore { get; set; }
    }

    private static readonly TestVector[] TestVectors =
    [
        // From https://github.com/multiformats/js-multihashing-async/blob/master/test/fixtures/encodes.js
        new() {
            Algorithm = "sha1",
            Input = "beep boop",
            Output = "11147c8357577f51d4f0a8d393aa1aaafb28863d9421"
        },
        new() {
            Algorithm = "sha2-256",
            Input = "beep boop",
            Output = "122090ea688e275d580567325032492b597bc77221c62493e76330b85ddda191ef7c"
        },
        new() {
            Algorithm = "sha2-512",
            Input = "beep boop",
            Output = "134014f301f31be243f34c5668937883771fa381002f1aaa5f31b3f78e500b66ff2f4f8ea5e3c9f5a61bd073e2452c480484b02e030fb239315a2577f7ae156af177"
        },
        new() {
            Algorithm = "sha3-512",
            Input = "beep boop",
            Output = "1440fae2c9eb19906057f8bf507f0e73ee02bb669d58c3069e7718b89ca4d314cf4fd6f1679019cc46d185c7af34f6c05a307b070e74e9ed5b9c64f86aacc2b90d10"
        },
        new() {
            Algorithm = "sha3-384",
            Input = "beep boop",
            Output = "153075a9cff1bcfbe8a7025aa225dd558fb002769d4bf3b67d2aaf180459172208bea989804aefccf060b583e629e5f41e8d"
        },
        new() {
            Algorithm = "sha3-256",
            Input = "beep boop",
            Output = "1620828705da60284b39de02e3599d1f39e6c1df001f5dbf63c9ec2d2c91a95a427f"
        },
        new() {
            Algorithm = "sha3-224",
            Input = "beep boop",
            Output = "171c0da73a89549018df311c0a63250e008f7be357f93ba4e582aaea32b8"
        },
        new() {
            Algorithm = "shake-128",
            Input = "beep boop",
            Output = "18105fe422311f770743c2e0d86bcca09211"
        },
        new() {
            Algorithm = "shake-256",
            Input = "beep boop",
            Output = "192059feb5565e4f924baef74708649fed376d63948a862322ed763ecf093b63b38b"
        },
        new() {
            Algorithm = "keccak-224",
            Input = "beep boop",
            Output = "1a1c2bd72cde2f75e523512999eb7639f17b699efe29bec342f5a0270896"
        },
        new() {
            Algorithm = "keccak-256",
            Input = "beep boop",
            Output = "1b20ee6f6b4ce5d754c01203cb3b99294b88615df5999e20d6fe509204fa254a0f97"
        },
        new() {
            Algorithm = "keccak-384",
            Input = "beep boop",
            Output = "1c300e2fcca40e861fc425a2503a65f4a4befab7be7f193e57654ca3713e85262b035e54d5ade93f9632b810ab88b04f7d84"
        },
        new() {
            Algorithm = "keccak-512",
            Input = "beep boop",
            Output = "1d40e161c54798f78eba3404ac5e7e12d27555b7b810e7fd0db3f25ffa0c785c438331b0fbb6156215f69edf403c642e5280f4521da9bd767296ec81f05100852e78"
        },
        new() {
            Algorithm = "blake2b-512",
            Input = "beep boop",
            Output = "c0e402400eac6255ba822373a0948122b8d295008419a8ab27842ee0d70eca39855621463c03ec75ac3610aacfdff89fa989d8d61fc00450148f289eb5b12ad1a954f659"
        },
        new() {
            Algorithm = "blake2b-160",
            Input = "beep boop",
            Output = "94e40214fe303247293e54e0a7ea48f9408ca68b36b08442"
        },
        new() {
            Algorithm = "blake2s-256",
            Input = "beep boop",
            Output = "e0e402204542eaca484e4311def8af74b546edd7fceb49eeb3cdcfd8a4a72ed0dc81d4c0"
        },
        new() {
            Algorithm = "dbl-sha2-256",
            Input = "beep boop",
            Output = "56209cd9115d76945c2455b1450295b05f4edeba2e7286bc24c23e266b48faf578c0"
        },
        new() {
            Algorithm = "identity",
            Input = "ab",
            Output = "00026162"
        },
        new() {
            Algorithm = "id",
            Input = "ab",
            Output = "00026162"
        },
    ];

    /// <summary>
    /// Tests the test vectors.
    /// </summary>
    [TestMethod]
    public void CheckMultiHash()
    {
        foreach (TestVector v in TestVectors)
        {
            if (v.Ignore)
            {
                continue;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(v.Input!);
            var mh = MultiHash.ComputeHash(bytes, v.Algorithm!);
            Assert.AreEqual(v.Output, mh.ToArray().ToHexString(), v.Algorithm);
        }
    }

    /// <summary>
    /// Tests the test vectors using streams.
    /// </summary>
    [TestMethod]
    public void CheckMultiHash_Stream()
    {
        foreach (TestVector v in TestVectors)
        {
            if (v.Ignore)
            {
                continue;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(v.Input!);
            using var ms = new MemoryStream(bytes, false);
            var mh = MultiHash.ComputeHash(ms, v.Algorithm!);
            Assert.AreEqual(v.Output, mh.ToArray().ToHexString(), v.Algorithm);
        }
    }

    /// <summary>
    /// Tests the identity hash.
    /// </summary>
    [TestMethod]
    public void IdentityHash()
    {
        byte[] hello = Encoding.UTF8.GetBytes("Hello, world.");
        var mh = MultiHash.ComputeHash(hello);
        Assert.IsFalse(mh.IsIdentityHash);
        CollectionAssert.AreNotEqual(hello, mh.Digest);

        mh = MultiHash.ComputeHash(hello, "identity");
        Assert.IsTrue(mh.IsIdentityHash);
        CollectionAssert.AreEqual(hello, mh.Digest);

        var mh1 = new MultiHash(mh.ToBase58());
        Assert.AreEqual(mh, mh1);

        mh = MultiHash.ComputeHash(hello, "id");
        Assert.IsTrue(mh.IsIdentityHash);
        CollectionAssert.AreEqual(hello, mh.Digest);

        mh1 = new MultiHash(mh.ToBase58());
        Assert.AreEqual(mh, mh1);
    }

    /// <summary>
    /// Tests binary encoding and decoding.
    /// </summary>
    [TestMethod]
    public void Binary()
    {
        var mh = new MultiHash("QmPZ9gcCEpqKTo6aq61g2nXGUhM4iCL3ewB6LDXZCtioEB");
        Assert.AreEqual("sha2-256", mh.Algorithm.Name);
        Assert.HasCount(32, mh.Digest);

        byte[] binary = mh.ToArray();
        var mh1 = new MultiHash(binary);
        Assert.AreEqual(mh.Algorithm.Name, mh1.Algorithm.Name);
        CollectionAssert.AreEqual(mh.Digest, mh1.Digest);
    }

    /// <summary>
    /// Tests JSON serialization and deserialization.
    /// </summary>
    [TestMethod]
    public void JsonSerialization()
    {
        var a = new MultiHash("QmPZ9gcCEpqKTo6aq61g2nXGUhM4iCL3ewB6LDXZCtioEB");
        string json = JsonConvert.SerializeObject(a);
        Assert.AreEqual($"\"{a}\"", json);
        MultiHash? b = JsonConvert.DeserializeObject<MultiHash>(json);
        Assert.AreEqual(a, b);

        a = null;
        json = JsonConvert.SerializeObject(a);
        b = JsonConvert.DeserializeObject<MultiHash>(json);
        Assert.IsNull(b);
    }

    /// <summary>
    /// Tests getting the name of a hash algorithm from its code.
    /// </summary>
    [TestMethod]
    public void CodeToName()
    {
        Assert.AreEqual("sha2-512", MultiHash.GetHashAlgorithmName(0x13));
        _ = ExceptionAssert.Throws<KeyNotFoundException>(
            () => MultiHash.GetHashAlgorithmName(0xbadbad));
    }

    /// <summary>
    /// Tests getting the <see cref="System.Security.Cryptography.HashAlgorithm"/> for a hash algorithm name.
    /// </summary>
    [TestMethod]
    public void GetAlgorithmByName()
    {
        Assert.IsNotNull(MultiHash.GetHashAlgorithm());
        Assert.IsNotNull(MultiHash.GetHashAlgorithm("sha2-512"));
        KeyNotFoundException e = ExceptionAssert.Throws<KeyNotFoundException>(() =>
        {
            System.Security.Cryptography.HashAlgorithm _ = MultiHash.GetHashAlgorithm("unknown");
        });
    }
}
