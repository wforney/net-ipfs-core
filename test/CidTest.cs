using System.Text;
using Google.Protobuf;
using Newtonsoft.Json;

namespace Ipfs;

/// <summary>
/// Tests for <see cref="Cid"/>.
/// </summary>
[TestClass]
public class CidTest
{
    /// <summary>
    /// Tests the default format of <see cref="Cid.ToString()"/>.
    /// </summary>
    [TestMethod]
    public void ToString_Default()
    {
        var cid = new Cid { Hash = new MultiHash("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V") };
        Assert.AreEqual("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V", cid.ToString());

        cid = "zBunRGrmCGokA1oMESGGTfrtcMFsVA8aEtcNzM54akPWXF97uXCqTjF3GZ9v8YzxHrG66J8QhtPFWwZebRZ2zeUEELu67";
        Assert.AreEqual("zBunRGrmCGokA1oMESGGTfrtcMFsVA8aEtcNzM54akPWXF97uXCqTjF3GZ9v8YzxHrG66J8QhtPFWwZebRZ2zeUEELu67", cid.ToString());
    }

    /// <summary>
    /// Tests the long format of <see cref="Cid.ToString(string)"/>.
    /// </summary>
    [TestMethod]
    public void ToString_L()
    {
        var cid = new Cid { Hash = new MultiHash("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V") };
        Assert.AreEqual("base58btc cidv0 dag-pb sha2-256 QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V", cid.ToString("L"));

        cid = "zBunRGrmCGokA1oMESGGTfrtcMFsVA8aEtcNzM54akPWXF97uXCqTjF3GZ9v8YzxHrG66J8QhtPFWwZebRZ2zeUEELu67";
        Assert.AreEqual("base58btc cidv1 dag-pb sha2-512 8Vx9QNCcSt39anEamkkSaNw5rDHQ7yuadq7ihZed477qQNXxYr3HReMamd1Q2EnUeL4oNtVAmNw1frEhEN1aoqFuKD", cid.ToString("L"));
    }

    /// <summary>
    /// Tests the general format of <see cref="Cid.ToString(string)"/>.
    /// </summary>
    [TestMethod]
    public void ToString_G()
    {
        var cid = new Cid { Hash = new MultiHash("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V") };
        Assert.AreEqual("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V", cid.ToString("G"));

        cid = "zBunRGrmCGokA1oMESGGTfrtcMFsVA8aEtcNzM54akPWXF97uXCqTjF3GZ9v8YzxHrG66J8QhtPFWwZebRZ2zeUEELu67";
        Assert.AreEqual("zBunRGrmCGokA1oMESGGTfrtcMFsVA8aEtcNzM54akPWXF97uXCqTjF3GZ9v8YzxHrG66J8QhtPFWwZebRZ2zeUEELu67", cid.ToString("G"));
    }

    /// <summary>
    /// Tests that an invalid format throws <see cref="FormatException"/>.
    /// </summary>
    [TestMethod]
    public void ToString_InvalidFormat()
    {
        var cid = new Cid { Hash = new MultiHash("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V") };
        _ = ExceptionAssert.Throws<FormatException>(() => cid.ToString("?"));
    }

    /// <summary>
    /// Tests that a <see cref="MultiHash"/> can be implicitly converted to a <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void MultiHash_is_Cid_V0()
    {
        var mh = new MultiHash("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V");
        Cid cid = mh;
        Assert.AreEqual(0, cid.Version);
        Assert.AreEqual("dag-pb", cid.ContentType);
        Assert.AreEqual("base58btc", cid.Encoding);
        Assert.AreSame(mh, cid.Hash);
    }

    /// <summary>
    /// Tests that a <see cref="MultiHash"/> can be implicitly converted to a <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void MultiHash_is_Cid_V1()
    {
        byte[] hello = Encoding.UTF8.GetBytes("Hello, world.");
        var mh = MultiHash.ComputeHash(hello, "sha2-512");
        Cid cid = mh;
        Assert.AreEqual(1, cid.Version);
        Assert.AreEqual("dag-pb", cid.ContentType);
        Assert.AreEqual("base32", cid.Encoding);
        Assert.AreSame(mh, cid.Hash);
    }

    /// <summary>
    /// Tests <see cref="Cid.Encode"/> for version 0 CIDs.
    /// </summary>
    [TestMethod]
    public void Encode_V0()
    {
        string hash = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V";
        Cid cid = new MultiHash(hash);
        Assert.AreEqual(hash, cid.Encode());
        Assert.AreEqual(0, cid.Version);

        cid = new Cid
        {
            ContentType = "dag-pb",
            Encoding = "base58btc",
            Hash = hash
        };
        Assert.AreEqual(hash, cid.Encode());
        Assert.AreEqual(0, cid.Version);
    }

    /// <summary>
    /// Tests <see cref="Cid.Encode"/> for version 1 CIDs.
    /// </summary>
    [TestMethod]
    public void Encode_V1()
    {
        var cid = new Cid
        {
            Version = 1,
            ContentType = "raw",
            Encoding = "base58btc",
            Hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4"
        };
        Assert.AreEqual("zb2rhj7crUKTQYRGCRATFaQ6YFLTde2YzdqbbhAASkL9uRDXn", cid.Encode());

        cid = new Cid
        {
            ContentType = "raw",
            Hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4"
        };
        Assert.AreEqual(1, cid.Version);
        Assert.AreEqual("base32", cid.Encoding);
        Assert.AreEqual("bafkreifzjut3te2nhyekklss27nh3k72ysco7y32koao5eei66wof36n5e", cid.Encode());
    }

    /// <summary>
    /// Tests <see cref="Cid.Encode"/> for version 0 CIDs that are upgraded to version 1.
    /// </summary>
    [TestMethod]
    public void Encode_Upgrade_to_V1_ContentType()
    {
        var cid = new Cid
        {
            ContentType = "raw",
            Hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4"
        };
        Assert.AreEqual(1, cid.Version);
        Assert.AreEqual("base32", cid.Encoding);
        Assert.AreEqual("bafkreifzjut3te2nhyekklss27nh3k72ysco7y32koao5eei66wof36n5e", cid.Encode());
    }

    /// <summary>
    /// Tests <see cref="Cid.Encode"/> for version 0 CIDs that are upgraded to version 1.
    /// </summary>
    [TestMethod]
    public void Encode_Upgrade_to_V1_Encoding()
    {
        var cid = new Cid
        {
            Encoding = "base64",
            Hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4"
        };
        Assert.AreEqual(1, cid.Version);
        Assert.AreEqual("mAXASILlNJ7mTTT4IpS5S19p9q/rEhO/jelOA7pCI96zi783p", cid.Encode());
    }

    /// <summary>
    /// Tests <see cref="Cid.Encode"/> for version 0 CIDs that are upgraded to version 1.
    /// </summary>
    [TestMethod]
    public void Encode_Upgrade_to_V1_Hash()
    {
        byte[] hello = Encoding.UTF8.GetBytes("Hello, world.");
        var mh = MultiHash.ComputeHash(hello, "sha2-512");
        var cid = new Cid
        {
            Hash = mh
        };
        Assert.AreEqual(1, cid.Version);
        Assert.AreEqual("base32", cid.Encoding);
        Assert.AreEqual("bafybgqfnbq34ghljwmk7hka7cpem3zybbffnsfzfxinq3qyztsuxcntbxaua23xx42hrgptcchrolkndcucelv3pc4eoarjbwdxagtylboxsm", cid.Encode());
    }

    /// <summary>
    /// Tests that an invalid content type throws <see cref="KeyNotFoundException"/>.
    /// </summary>
    [TestMethod]
    public void Encode_V1_Invalid_ContentType()
    {
        var cid = new Cid
        {
            Version = 1,
            ContentType = "unknown",
            Encoding = "base58btc",
            Hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4"
        };
        _ = Assert.ThrowsException<KeyNotFoundException>(cid.Encode);
    }

    /// <summary>
    /// Tests that an invalid encoding throws <see cref="KeyNotFoundException"/>.
    /// </summary>
    [TestMethod]
    public void Encode_V1_Invalid_Encoding()
    {
        var cid = new Cid
        {
            Version = 1,
            ContentType = "raw",
            Encoding = "unknown",
            Hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4"
        };
        _ = Assert.ThrowsException<KeyNotFoundException>(cid.Encode);
    }

    /// <summary>
    /// Tests <see cref="Cid.Decode"/> for version 0 CIDs.
    /// </summary>
    [TestMethod]
    public void Decode_V0()
    {
        string hash = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V";
        var cid = Cid.Decode(hash);
        Assert.AreEqual(0, cid.Version);
        Assert.AreEqual("dag-pb", cid.ContentType);
        Assert.AreEqual("base58btc", cid.Encoding);
        Assert.AreEqual(hash, cid.Encode());
    }

    /// <summary>
    /// Tests that an invalid version 0 CID throws <see cref="FormatException"/>.
    /// </summary>
    [TestMethod]
    public void Decode_V0_Invalid()
    {
        string hash = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39?";
        _ = Assert.ThrowsException<FormatException>(() => Cid.Decode(hash));
    }

    /// <summary>
    /// Tests that an invalid version throws <see cref="FormatException"/>.
    /// </summary>
    [TestMethod]
    public void Decode_Invalid_Version()
    {
        var cid = new Cid
        {
            Version = 32767,
            ContentType = "raw",
            Encoding = "base58btc",
            Hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4"
        };
        string s = cid.Encode();
        _ = Assert.ThrowsException<FormatException>(() => Cid.Decode(s));
    }

    /// <summary>
    /// Tests <see cref="Cid.Decode"/> for version 1 CIDs.
    /// </summary>
    [TestMethod]
    public void Decode_V1()
    {
        string id = "zb2rhj7crUKTQYRGCRATFaQ6YFLTde2YzdqbbhAASkL9uRDXn";
        string hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        var cid = Cid.Decode(id);
        Assert.AreEqual(1, cid.Version);
        Assert.AreEqual("base58btc", cid.Encoding);
        Assert.AreEqual("raw", cid.ContentType);
        Assert.AreEqual(hash, cid.Hash);
    }

    /// <summary>
    /// Tests <see cref="Cid.Decode"/> for version 1 CIDs with an unknown content type.
    /// </summary>
    [TestMethod]
    public void Decode_V1_Unknown_ContentType()
    {
        string id = "zJAFhtPN28kqMxDkZawWCCL52BzaiymqFgX3LA7XzkNRMNAN1T1J";
        string hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        var cid = Cid.Decode(id);
        Assert.AreEqual(1, cid.Version);
        Assert.AreEqual("base58btc", cid.Encoding);
        Assert.AreEqual("codec-32767", cid.ContentType);
        Assert.AreEqual(hash, cid.Hash);
    }

    /// <summary>
    /// Tests that an invalid version 1 CID throws <see cref="FormatException"/>.
    /// </summary>
    [TestMethod]
    public void Decode_V1_Invalid_MultiBase_String()
    {
        string id = "zb2rhj7crUKTQYRGCRATFaQ6YFLTde2YzdqbbhAASkL9uRDX?";
        _ = Assert.ThrowsException<FormatException>(() => Cid.Decode(id));
    }

    /// <summary>
    /// Tests that an invalid version 1 CID throws <see cref="FormatException"/>.
    /// </summary>
    [TestMethod]
    public void Decode_V1_Invalid_MultiBase_Code()
    {
        string id = "?";
        _ = Assert.ThrowsException<FormatException>(() => Cid.Decode(id));
    }

    /// <summary>
    /// Tests the equality members of <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void Value_Equality()
    {
        var a0 = Cid.Decode("zb2rhj7crUKTQYRGCRATFaQ6YFLTde2YzdqbbhAASkL9uRDXn");
        var a1 = Cid.Decode("zb2rhj7crUKTQYRGCRATFaQ6YFLTde2YzdqbbhAASkL9uRDXn");
        var b = Cid.Decode("QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L5");
        Cid? c = null;
        Cid? d = null;

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

        Assert.IsFalse(a0 != a0);
        Assert.IsFalse(a0 != a1);
        Assert.IsTrue(a0 != b);

        Assert.IsTrue(a0.Equals(a0));
        Assert.IsTrue(a0.Equals(a1));
        Assert.IsFalse(a0.Equals(b));

        Assert.AreEqual(a0, a0);
        Assert.AreEqual(a0, a1);
        Assert.AreNotEqual(a0, b);

        Assert.AreEqual<Cid>(a0, a0);
        Assert.AreEqual<Cid>(a0, a1);
        Assert.AreNotEqual<Cid>(a0, b);

        Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
        Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
        Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    /// Tests the implicit conversion between <see cref="string"/> and <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void Implicit_Conversion_From_V0_String()
    {
        string hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        Cid cid = hash;
        Assert.AreEqual(0, cid.Version);
        Assert.AreEqual("dag-pb", cid.ContentType);
        Assert.AreEqual("base58btc", cid.Encoding);
        Assert.AreEqual(hash, cid.Encode());
    }

    /// <summary>
    /// Tests the implicit conversion between <see cref="string"/> and <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void Implicit_Conversion_From_V1_String()
    {
        string id = "zb2rhj7crUKTQYRGCRATFaQ6YFLTde2YzdqbbhAASkL9uRDXn";
        string hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        Cid cid = id;
        Assert.AreEqual(1, cid.Version);
        Assert.AreEqual("base58btc", cid.Encoding);
        Assert.AreEqual("raw", cid.ContentType);
        Assert.AreEqual(hash, cid.Hash);
    }

    /// <summary>
    /// Tests the implicit conversion between <see cref="string"/> and <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void Implicit_Conversion_To_String()
    {
        string id = "zb2rhj7crUKTQYRGCRATFaQ6YFLTde2YzdqbbhAASkL9uRDXn";
        Cid cid = id;
        string s = cid;
        Assert.AreEqual(id, s);
    }

    /// <summary>
    /// Tests streaming a <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void Streaming_V0()
    {
        Cid cid = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        var stream = new MemoryStream();
        cid.Write(stream);
        stream.Position = 0;
        var clone = Cid.Read(stream);
        Assert.AreEqual(cid.Version, clone.Version);
        Assert.AreEqual(cid.ContentType, clone.ContentType);
        Assert.AreEqual(cid.Hash, clone.Hash);
    }

    /// <summary>
    /// Tests streaming a <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void Streaming_V1()
    {
        Cid cid = "zBunRGrmCGokA1oMESGGTfrtcMFsVA8aEtcNzM54akPWXF97uXCqTjF3GZ9v8YzxHrG66J8QhtPFWwZebRZ2zeUEELu67";
        var stream = new MemoryStream();
        cid.Write(stream);
        stream.Position = 0;
        var clone = Cid.Read(stream);
        Assert.AreEqual(cid.Version, clone.Version);
        Assert.AreEqual(cid.ContentType, clone.ContentType);
        Assert.AreEqual(cid.Hash, clone.Hash);
    }

    /// <summary>
    /// Tests streaming a <see cref="Cid"/> using Protocol Buffers.
    /// </summary>
    [TestMethod]
    public void Protobuf_V0()
    {
        Cid cid = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        var stream = new MemoryStream();
        var cos = new CodedOutputStream(stream);
        cid.Write(cos);
        cos.Flush();
        stream.Position = 0;
        var cis = new CodedInputStream(stream);
        var clone = Cid.Read(cis);
        Assert.AreEqual(cid.Version, clone.Version);
        Assert.AreEqual(cid.ContentType, clone.ContentType);
        Assert.AreEqual(cid.Hash, clone.Hash);
    }

    /// <summary>
    /// Tests streaming a <see cref="Cid"/> using Protocol Buffers.
    /// </summary>
    [TestMethod]
    public void Protobuf_V1()
    {
        Cid cid = "zBunRGrmCGokA1oMESGGTfrtcMFsVA8aEtcNzM54akPWXF97uXCqTjF3GZ9v8YzxHrG66J8QhtPFWwZebRZ2zeUEELu67";
        var stream = new MemoryStream();
        var cos = new CodedOutputStream(stream);
        cid.Write(cos);
        cos.Flush();
        stream.Position = 0;
        var cis = new CodedInputStream(stream);
        var clone = Cid.Read(cis);
        Assert.AreEqual(cid.Version, clone.Version);
        Assert.AreEqual(cid.ContentType, clone.ContentType);
        Assert.AreEqual(cid.Hash, clone.Hash);
    }

    /// <summary>
    /// Tests that a <see cref="Cid"/> is immutable.
    /// </summary>
    [TestMethod]
    public void Immutable()
    {
        Cid cid = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        Assert.AreEqual("QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4", cid.Encode());
        _ = ExceptionAssert.Throws<NotSupportedException>(() => cid.ContentType = "dag-cbor");
        _ = ExceptionAssert.Throws<NotSupportedException>(() => cid.Encoding = "base64");
        _ = ExceptionAssert.Throws<NotSupportedException>(() => cid.Hash = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L5");
        _ = ExceptionAssert.Throws<NotSupportedException>(() => cid.Version = 0);
    }

    private class CidAndX
    {
        public Cid? Cid;
        public int X;
    }

    /// <summary>
    /// Tests JSON serialization of <see cref="Cid"/>.
    /// </summary>
    [TestMethod]
    public void JsonSerialization()
    {
        Cid? a = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4";
        string json = JsonConvert.SerializeObject(a);
        Assert.AreEqual($"\"{a.Encode()}\"", json);
        Cid? b = JsonConvert.DeserializeObject<Cid>(json);
        Assert.AreEqual(a, b);

        a = null;
        json = JsonConvert.SerializeObject(a);
        b = JsonConvert.DeserializeObject<Cid>(json);
        Assert.IsNull(b);

        var x = new CidAndX { Cid = "QmaozNR7DZHQK1ZcU9p7QdrshMvXqWK6gpu5rmrkPdT3L4", X = 42 };
        json = JsonConvert.SerializeObject(x);
        CidAndX? y = JsonConvert.DeserializeObject<CidAndX>(json);
        Assert.IsNotNull(y);
        Assert.AreEqual(x.Cid, y!.Cid);
        Assert.AreEqual(x.X, y.X);

        x.Cid = null;
        json = JsonConvert.SerializeObject(x);
        y = JsonConvert.DeserializeObject<CidAndX>(json);
        Assert.IsNotNull(y);
        Assert.AreEqual(x.Cid, y!.Cid);
        Assert.AreEqual(x.X, y.X);
    }

    /// <summary>
    /// Tests byte array serialization of <see cref="Cid"/> version 1.
    /// </summary>
    [TestMethod]
    public void ByteArrays_V1()
    {
        Cid cid = "zBunRGrmCGokA1oMESGGTfrtcMFsVA8aEtcNzM54akPWXF97uXCqTjF3GZ9v8YzxHrG66J8QhtPFWwZebRZ2zeUEELu67";
        byte[] buffer = cid.ToArray();
        var clone = Cid.Read(buffer);
        Assert.AreEqual(cid.Version, clone.Version);
        Assert.AreEqual(cid.ContentType, clone.ContentType);
        Assert.AreEqual(cid.Hash.Algorithm.Name, clone.Hash.Algorithm.Name);
        Assert.AreEqual(cid.Hash, clone.Hash);
    }

    /// <summary>
    /// Tests byte array serialization of <see cref="Cid"/> version 0.
    /// </summary>
    [TestMethod]
    public void ByteArrays_V0()
    {
        byte[] buffer = "1220a4edf38611d7d4a2d3ff2d97f88a7256eba31b57982f803b4de7bbeb0343c37b".ToHexBuffer();
        var cid = Cid.Read(buffer);
        Assert.AreEqual(0, cid.Version);
        Assert.AreEqual("dag-pb", cid.ContentType);
        Assert.AreEqual("QmZSU1xNFsBtCnzK2Nk9N4bAxQiVNdmugU9DQDE3ntkTpe", cid.Hash.ToString());

        byte[] clone = cid.ToArray();
        CollectionAssert.AreEqual(buffer, clone);
    }
}
