using System.Net;
using Newtonsoft.Json;

namespace Ipfs;

/// <summary>
/// Tests the <see cref="MultiAddress"/> class.
/// </summary>
[TestClass]
public class MultiAddressTest
{
    private const string Somewhere = "/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC";
    private const string Nowhere = "/ip4/10.1.10.11/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC";

    /// <summary>
    /// Tests parsing a multiaddress.
    /// </summary>
    [TestMethod]
    public void Parsing()
    {
        var a = new MultiAddress(Somewhere);
        Assert.HasCount(3, a.Protocols);
        Assert.AreEqual("ip4", a.Protocols[0].Name);
        Assert.AreEqual("10.1.10.10", a.Protocols[0].Value);
        Assert.AreEqual("tcp", a.Protocols[1].Name);
        Assert.AreEqual("29087", a.Protocols[1].Value);
        Assert.AreEqual("ipfs", a.Protocols[2].Name);
        Assert.AreEqual("QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC", a.Protocols[2].Value);

        Assert.IsEmpty(new MultiAddress((string)null!).Protocols);
        Assert.IsEmpty(new MultiAddress("").Protocols);
        Assert.IsEmpty(new MultiAddress("  ").Protocols);
    }

    /// <summary>
    /// Tests parsing a multiaddress with an unknown protocol name.
    /// </summary>
    [TestMethod]
    public void Unknown_Protocol_Name() => ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/foobar/123"));

    /// <summary>
    /// Tests parsing a multiaddress with a missing protocol name.
    /// </summary>
    [TestMethod]
    public void Missing_Protocol_Name() => ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/"));

    /// <summary>
    /// Tests the <see cref="MultiAddress.ToString"/> method.
    /// </summary>
    [TestMethod]
    public new void ToString() => Assert.AreEqual(Somewhere, new MultiAddress(Somewhere).ToString());

    /// <summary>
    /// Tests value equality.
    /// </summary>
    [TestMethod]
    public void Value_Equality()
    {
        var a0 = new MultiAddress(Somewhere);
        var a1 = new MultiAddress(Somewhere);
        var b = new MultiAddress(Nowhere);
        MultiAddress? c = null;
        MultiAddress? d = null;

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

        Assert.AreEqual(a0, a0);
        Assert.AreEqual(a0, a1);
        Assert.AreNotEqual(a0, b);

        Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
        Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
        Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    /// Tests parsing a multiaddress with a bad port number.
    /// </summary>
    [TestMethod]
    public void Bad_Port()
    {
        var tcp = new MultiAddress("/tcp/65535");
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/tcp/x"));
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/tcp/65536"));

        var udp = new MultiAddress("/udp/65535");
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/upd/x"));
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/udp/65536"));
    }

    /// <summary>
    /// Tests parsing a multiaddress with a bad IP address.
    /// </summary>
    [TestMethod]
    public void Bad_IPAddress()
    {
        var ipv4 = new MultiAddress("/ip4/127.0.0.1");
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/ip4/x"));
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/ip4/127."));
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/ip4/::1"));

        var ipv6 = new MultiAddress("/ip6/::1");
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/ip6/x"));
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/ip6/03:"));
        _ = ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("/ip6/127.0.0.1"));
    }

    /// <summary>
    /// Tests parsing a multiaddress with a bad onion address.
    /// </summary>
    [TestMethod]
    public void Bad_Onion_MultiAdress()
    {
        string[] badCases =
        [
            "/onion/9imaq4ygg2iegci7:80",
            "/onion/aaimaq4ygg2iegci7:80",
            "/onion/timaq4ygg2iegci7:0",
            "/onion/timaq4ygg2iegci7:-1",
            "/onion/timaq4ygg2iegci7",
            "/onion/timaq4ygg2iegci@:666",
        ];
        foreach (string badCase in badCases)
        {
            _ = ExceptionAssert.Throws<Exception, MultiAddress>(() => new MultiAddress(badCase));
        }
    }

    /// <summary>
    /// Tests round tripping a multiaddress through its various representations.
    /// </summary>
    [TestMethod]
    public void RoundTripping()
    {
        string[] addresses =
        [
            Somewhere,
            "/ip4/1.2.3.4/tcp/80/http",
            "/ip6/3ffe:1900:4545:3:200:f8ff:fe21:67cf/tcp/443/https",
            "/ip6/3ffe:1900:4545:3:200:f8ff:fe21:67cf/udp/8001",
            "/ip6/3ffe:1900:4545:3:200:f8ff:fe21:67cf/sctp/8001",
            "/ip6/3ffe:1900:4545:3:200:f8ff:fe21:67cf/dccp/8001",
            "/ip4/1.2.3.4/tcp/80/ws",
            "/libp2p-webrtc-star/ip4/127.0.0.1/tcp/9090/ws/ipfs/QmcgpsyWgH8Y8ajJz1Cu72KnS5uo2Aa2LpzU7kinSupNKC",
            "/ip4/127.0.0.1/tcp/1234/ipfs/QmcgpsyWgH8Y8ajJz1Cu72KnS5uo2Aa2LpzU7kinSupNKC",
            "/ip4/1.2.3.4/tcp/80/udt",
            "/ip4/1.2.3.4/tcp/80/utp",
            "/onion/aaimaq4ygg2iegci:80",
            "/onion/timaq4ygg2iegci7:80/http",
            "/p2p-circuit/ipfs/QmcgpsyWgH8Y8ajJz1Cu72KnS5uo2Aa2LpzU7kinSupNKC",
            "/dns/ipfs.io",
            "/dns4/ipfs.io",
            "/dns6/ipfs.io",
            "/dns4/wss0.bootstrap.libp2p.io/tcp/443/wss/ipfs/QmZMxNdpMkewiVZLMRxaNxUeZpDUb34pWjZ1kZvsd16Zic",
            "/ip4/127.0.0.0/ipcidr/16",
            "/p2p/QmNnooDu7bfjPFoTZYxMNLWUQJyrVwtbZg5gBMjTezGAJN",
            "/ip4/127.0.0.1/udp/4023/quic",
        ];
        foreach (string? a in addresses)
        {
            var ma0 = new MultiAddress(a);

            var ms = new MemoryStream();
            ma0.Write(ms);
            ms.Position = 0;
            var ma1 = new MultiAddress(ms);
            Assert.AreEqual(ma0, ma1);

            var ma2 = new MultiAddress(ma0.ToString());
            Assert.AreEqual(ma0, ma2);

            var ma3 = new MultiAddress(ma0.ToArray());
            Assert.AreEqual(ma0, ma3);
        }
    }

    /// <summary>
    /// Tests parsing a multiaddress with an invalid protocol code.
    /// </summary>
    [TestMethod]
    public void Reading_Invalid_Code() => ExceptionAssert.Throws<InvalidDataException, MultiAddress>(static () => new MultiAddress([0x7F]));

    /// <summary>
    /// Tests parsing a multiaddress with invalid text.
    /// </summary>
    [TestMethod]
    public void Reading_Invalid_Text() => ExceptionAssert.Throws<FormatException, MultiAddress>(static () => new MultiAddress("tcp/80"));

    /// <summary>
    /// Tests the implicit conversion from <see cref="string"/> to <see cref="MultiAddress"/>.
    /// </summary>
    [TestMethod]
    public void Implicit_Conversion_From_String()
    {
        MultiAddress a = Somewhere;
        Assert.IsInstanceOfType<MultiAddress>(a);
    }

    /// <summary>
    /// Tests the wire formats of various multiaddresses.
    /// </summary>
    [TestMethod]
    public void Wire_Formats()
    {
        Assert.AreEqual("047f000001910204d2", new MultiAddress("/ip4/127.0.0.1/udp/1234").ToArray().ToHexString());
        Assert.AreEqual("047f000001910204d2047f0000010610e1", new MultiAddress("/ip4/127.0.0.1/udp/1234/ip4/127.0.0.1/tcp/4321").ToArray().ToHexString());
        Assert.AreEqual("29200108a07ac542013ac986fffe317095", new MultiAddress("/ip6/2001:8a0:7ac5:4201:3ac9:86ff:fe31:7095").ToArray().ToHexString());
        Assert.AreEqual("a503221220d52ebb89d85b02a284948203a62ff28389c57c9f42beec4ec20db76a68911c0b", new MultiAddress("/ipfs/QmcgpsyWgH8Y8ajJz1Cu72KnS5uo2Aa2LpzU7kinSupNKC").ToArray().ToHexString());
        Assert.AreEqual("047f000001910204d2ae02", new MultiAddress("/ip4/127.0.0.1/udp/1234/utp").ToArray().ToHexString());
        Assert.AreEqual("bc030010c0439831b48218480050", new MultiAddress("/onion/aaimaq4ygg2iegci:80").ToArray().ToHexString());
    }

    /// <summary>
    /// Tests extracting the peer ID from a multiaddress.
    /// </summary>
    [TestMethod]
    public void PeerID_With_ipfs()
    {
        var ma = new MultiAddress("/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC");
        Assert.AreEqual("QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC", ma.PeerId.ToBase58());
    }

    /// <summary>
    /// Tests extracting the peer ID from a multiaddress.
    /// </summary>
    [TestMethod]
    public void PeerID_With_p2p()
    {
        var ma = new MultiAddress("/ip4/10.1.10.10/tcp/29087/p2p/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC");
        Assert.AreEqual("QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC", ma.PeerId.ToBase58());
    }

    /// <summary>
    /// Tests that multiaddresses with /ipfs/ and /p2p/ are considered equal.
    /// </summary>
    [TestMethod]
    public void PeerID_ipfs_p2p_are_equal()
    {
        var ipfs = new MultiAddress("/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC");
        var p2p = new MultiAddress("/ip4/10.1.10.10/tcp/29087/p2p/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC");
        Assert.AreEqual(ipfs, p2p);

        var p2p1 = new MultiAddress("/ip4/10.1.10.10/tcp/29087/p2p/QmVCSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC");
        Assert.AreNotEqual(p2p, p2p1);

        var p2p2 = new MultiAddress("/p2p/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC");
        Assert.AreNotEqual(p2p, p2p2);
    }

    /// <summary>
    /// Tests extracting the peer ID from a multiaddress that does not have one.
    /// </summary>
    [TestMethod]
    public void PeerID_Missing()
    {
        var ma = new MultiAddress("/ip4/10.1.10.10/tcp/29087");
        _ = ExceptionAssert.Throws<Exception>(() => _ = ma.PeerId);
    }

    /// <summary>
    /// Tests the <see cref="MultiAddress.HasPeerId"/> property.
    /// </summary>
    [TestMethod]
    public void PeerId_IsPresent()
    {
        Assert.IsTrue(new MultiAddress("/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC").HasPeerId);
        Assert.IsTrue(new MultiAddress("/ip4/10.1.10.10/tcp/29087/p2p/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC").HasPeerId);
        Assert.IsFalse(new MultiAddress("/ip4/10.1.10.10/tcp/29087").HasPeerId);
    }

    /// <summary>
    /// Tests cloning a multiaddress.
    /// </summary>
    [TestMethod]
    public void Cloning()
    {
        var ma1 = new MultiAddress("/ip4/10.1.10.10/tcp/29087");
        MultiAddress ma2 = ma1.Clone();
        Assert.AreEqual(ma1, ma2);
        Assert.AreNotSame(ma1, ma2);
        Assert.AreNotSame(ma1.Protocols, ma2.Protocols);
        for (int i = 0; i < ma1.Protocols.Count; ++i)
        {
            NetworkProtocol p1 = ma1.Protocols[i];
            NetworkProtocol p2 = ma2.Protocols[i];
            Assert.AreEqual(p1.Code, p2.Code);
            Assert.AreEqual(p1.Name, p2.Name);
            Assert.AreEqual(p1.Value, p2.Value);
            Assert.AreNotSame(p1, p2);
        }
    }

    /// <summary>
    /// Tests that IPv6 scope IDs are ignored when comparing multiaddresses.
    /// </summary>
    [TestMethod]
    public void Ipv6ScopeId_Ignored()
    {
        var ma1 = new MultiAddress("/ip6/fe80::7573:b0a8:46b0:0bad%17/tcp/4009");
        var ma2 = new MultiAddress("/ip6/fe80::7573:b0a8:46b0:0bad/tcp/4009");
        Assert.AreEqual(ma2, ma1);
        Assert.AreEqual(ma2.ToString(), ma1.ToString());
    }

    /// <summary>
    /// Tests the <see cref="MultiAddress.TryCreate(string)"/> and <see cref="MultiAddress.TryCreate(byte[])"/> methods.
    /// </summary>
    [TestMethod]
    public void TryCreate_FromString()
    {
        Assert.IsNotNull(MultiAddress.TryCreate("/ip4/1.2.3.4/tcp/80"));
        Assert.IsNull(MultiAddress.TryCreate("/tcp/alpha")); // bad port
        Assert.IsNull(MultiAddress.TryCreate("/foobar")); // bad protocol
    }

    /// <summary>
    /// Tests the <see cref="MultiAddress.TryCreate(byte[])"/> method.
    /// </summary>
    [TestMethod]
    public void TryCreate_FromBytes()
    {
        var good = MultiAddress.TryCreate("/ip4/1.2.3.4/tcp/80");
        Assert.IsNotNull(good);
        var good1 = MultiAddress.TryCreate(good!.ToArray());
        Assert.AreEqual(good, good1);

        Assert.IsNull(MultiAddress.TryCreate([0x7f]));
    }

    /// <summary>
    /// Tests JSON serialization and deserialization.
    /// </summary>
    [TestMethod]
    public void JsonSerialization()
    {
        var a = new MultiAddress("/ip6/fe80::7573:b0a8:46b0:0bad/tcp/4009");
        string json = JsonConvert.SerializeObject(a);
        Assert.AreEqual($"\"{a}\"", json);
        MultiAddress? b = JsonConvert.DeserializeObject<MultiAddress>(json);
        Assert.IsNotNull(b);
        Assert.AreEqual(a.ToString(), b!.ToString());

        a = null;
        json = JsonConvert.SerializeObject(a);
        b = JsonConvert.DeserializeObject<MultiAddress>(json);
        Assert.IsNull(b);
    }

    /// <summary>
    /// Tests adding a peer ID to a multiaddress.
    /// </summary>
    [TestMethod]
    public void WithPeerId()
    {
        string id = "QmQusTXc1Z9C1mzxsqC9ZTFXCgSkpBRGgW4Jk2QYHxKE22";
        string id3 = "QmQusTXc1Z9C1mzxsqC9ZTFXCgSkpBRGgW4Jk2QYHxKE33";

        var ma1 = new MultiAddress("/ip4/127.0.0.1/tcp/4001");
        Assert.AreEqual($"{ma1}/p2p/{id}", ma1.WithPeerId(id));

        ma1 = new MultiAddress($"/ip4/127.0.0.1/tcp/4001/ipfs/{id}");
        Assert.AreSame(ma1, ma1.WithPeerId(id));

        ma1 = new MultiAddress($"/ip4/127.0.0.1/tcp/4001/p2p/{id}");
        Assert.AreSame(ma1, ma1.WithPeerId(id));

        _ = ExceptionAssert.Throws<Exception>(() =>
        {
            ma1 = new MultiAddress($"/ip4/127.0.0.1/tcp/4001/ipfs/{id3}");
            Assert.AreSame(ma1, ma1.WithPeerId(id));
        });
    }

    /// <summary>
    /// Tests removing a peer ID from a multiaddress.
    /// </summary>
    [TestMethod]
    public void WithoutPeerId()
    {
        string id = "QmQusTXc1Z9C1mzxsqC9ZTFXCgSkpBRGgW4Jk2QYHxKE22";

        var ma1 = new MultiAddress("/ip4/127.0.0.1/tcp/4001");
        Assert.AreSame(ma1, ma1.WithoutPeerId());

        ma1 = new MultiAddress($"/ip4/127.0.0.1/tcp/4001/ipfs/{id}");
        Assert.AreEqual("/ip4/127.0.0.1/tcp/4001", ma1.WithoutPeerId());

        ma1 = new MultiAddress($"/ip4/127.0.0.1/tcp/4001/p2p/{id}");
        Assert.AreEqual("/ip4/127.0.0.1/tcp/4001", ma1.WithoutPeerId());
    }

    /// <summary>
    /// Tests that multiaddresses with /ipfs/ and /p2p/ are considered equal.
    /// </summary>
    [TestMethod]
    public void Alias_Equality()
    {
        var a = new MultiAddress("/ipfs/QmQusTXc1Z9C1mzxsqC9ZTFXCgSkpBRGgW4Jk2QYHxKE22");
        var b = new MultiAddress("/p2p/QmQusTXc1Z9C1mzxsqC9ZTFXCgSkpBRGgW4Jk2QYHxKE22");

        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    /// Tests that calling <see cref="MultiAddress.GetHashCode"/> does not throw an exception when the
    /// multiaddress contains a null value.
    /// </summary>
    [TestMethod]
    public void GetHashCode_NullValue()
    {
        var a = new MultiAddress("/ip4/139.178.69.3/udp/4001/quic/p2p/QmdGQoGuK3pao6bRDqGSDvux5SFHa4kC2XNFfHFcvcbydY/p2p-circuit/ipfs/QmPJkpfUedzahgVAj6tTUa3DHKVkfTSyvUmnn1USFpiCaF");
        int _ = a.GetHashCode();
    }

    /// <summary>
    /// Tests creating a multiaddress from an <see cref="IPAddress"/>.
    /// </summary>
    [TestMethod]
    public void FromIPAddress()
    {
        var ma = new MultiAddress(IPAddress.Loopback);
        Assert.AreEqual("/ip4/127.0.0.1", ma.ToString());

        ma = new MultiAddress(IPAddress.IPv6Loopback);
        Assert.AreEqual("/ip6/::1", ma.ToString());
    }

    /// <summary>
    /// Tests creating a multiaddress from an <see cref="IPEndPoint"/>.
    /// </summary>
    [TestMethod]
    public void FromIPEndpoint()
    {
        var ma = new MultiAddress(new IPEndPoint(IPAddress.Loopback, 4001));
        Assert.AreEqual("/ip4/127.0.0.1/tcp/4001", ma.ToString());

        ma = new MultiAddress(new IPEndPoint(IPAddress.IPv6Loopback, 4002));
        Assert.AreEqual("/ip6/::1/tcp/4002", ma.ToString());
    }
}
