﻿namespace Ipfs;

/// <summary>
/// Unit tests for <see cref="Peer"/>.
/// </summary>
[TestClass]
public class PeerTest
{
    private const string MarsId = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";
    private const string PlutoId = "QmSoLPppuBtQSGwKDZT2M73ULpjvfd3aZ6ha4oFGL1KrGM";
    private const string MarsPublicKey = "CAASogEwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAKGUtbRQf+a9SBHFEruNAUatS/tsGUnHuCtifGrlbYPELD3UyyhWf/FYczBCavx3i8hIPEW2jQv4ehxQxi/cg9SHswZCQblSi0ucwTBFr8d40JEiyB9CcapiMdFQxdMgGvXEOQdLz1pz+UPUDojkdKZq8qkkeiBn7KlAoGEocnmpAgMBAAE=";
    private const string MarsAddress = "/ip4/10.1.10.10/tcp/29087/ipfs/QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";

    /// <summary>
    /// ToString returns the peer ID.
    /// </summary>
    [TestMethod]
    public new void ToString()
    {
        Assert.AreEqual(string.Empty, new Peer().ToString());
        Assert.AreEqual(MarsId, new Peer { Id = MarsId }.ToString());
    }

    /// <summary>
    /// Default values of a new Peer.
    /// </summary>
    [TestMethod]
    public void DefaultValues()
    {
        var peer = new Peer();
        Assert.IsNull(peer.Id);
        Assert.AreEqual(0, peer.Addresses.Count());
        Assert.AreEqual("unknown/0.0", peer.ProtocolVersion);
        Assert.AreEqual("unknown/0.0", peer.AgentVersion);
        Assert.IsNull(peer.PublicKey);
        Assert.IsFalse(peer.IsValid()); // missing peer ID
        Assert.IsNull(peer.ConnectedAddress);
        Assert.IsFalse(peer.Latency.HasValue);
    }

    /// <summary>
    /// A peer with a connected address and latency.
    /// </summary>
    [TestMethod]
    public void ConnectedPeer()
    {
        var peer = new Peer
        {
            ConnectedAddress = new MultiAddress(MarsAddress),
            Latency = TimeSpan.FromHours(3.03 * 2)
        };
        Assert.AreEqual(MarsAddress, peer.ConnectedAddress.ToString());
        Assert.AreEqual(3.03 * 2, peer.Latency.Value.TotalHours);
    }

    /// <summary>
    /// Validation of a peer.
    /// </summary>
    [TestMethod]
    public void Validation_No_Id()
    {
        var peer = new Peer();
        Assert.IsFalse(peer.IsValid());
    }

    /// <summary>
    /// Validation of a peer with just an ID.
    /// </summary>
    [TestMethod]
    public void Validation_With_Id()
    {
        Peer peer = MarsId;
        Assert.IsTrue(peer.IsValid());
    }

    /// <summary>
    /// Validation of a peer with an ID and public key.
    /// </summary>
    [TestMethod]
    public void Validation_With_Id_Pubkey()
    {
        var peer = new Peer
        {
            Id = MarsId,
            PublicKey = MarsPublicKey
        };
        Assert.IsTrue(peer.IsValid());
    }

    /// <summary>
    /// Validation of a peer with an ID and invalid public key.
    /// </summary>
    [TestMethod]
    public void Validation_With_Id_Invalid_Pubkey()
    {
        var peer = new Peer
        {
            Id = PlutoId,
            PublicKey = MarsPublicKey
        };
        Assert.IsFalse(peer.IsValid());
    }

    /// <summary>
    /// Value equality of peers.
    /// </summary>
    [TestMethod]
    public void Value_Equality()
    {
        var a0 = new Peer { Id = MarsId };
        var a1 = new Peer { Id = MarsId };
        var b = new Peer { Id = PlutoId };
        Peer? c = null;
        Peer? d = null;

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

        Assert.AreEqual<Peer>(a0, a0);
        Assert.AreEqual<Peer>(a0, a1);
        Assert.AreNotEqual<Peer>(a0, b);

        Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
        Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
        Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    /// Implicit conversion from string to Peer.
    /// </summary>
    [TestMethod]
    public void Implicit_Conversion_From_String()
    {
        Peer a = MarsId;
        Assert.IsInstanceOfType<Peer>(a);
    }
}
