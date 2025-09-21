﻿namespace Ipfs;

/// <summary>
/// A daemon node on the IPFS network.
/// </summary>
/// <remarks>Equality is based solely on the peer's <see cref="Id"/>.</remarks>
public class Peer : IEquatable<Peer>
{
    private const string Unknown = "unknown/0.0";
    private static readonly MultiAddress[] NoAddress = [];

    /// <summary>
    /// The multiple addresses of the node.
    /// </summary>
    /// <value>Where the peer can be found. The default is an empty sequence.</value>
    public IEnumerable<MultiAddress> Addresses { get; set; } = NoAddress;

    /// <summary>
    /// The name and version of the IPFS software.
    /// </summary>
    /// <value>For example "go-ipfs/0.4.17/".</value>
    /// <remarks>
    /// There is no specification that describes the agent version string. The default is "unknown/0.0".
    /// </remarks>
    public string AgentVersion { get; set; } = Unknown;

    /// <summary>
    /// The <see cref="MultiAddress"/> that the peer is connected on.
    /// </summary>
    /// <value><b>null</b> when the peer is not connected to.</value>
    public MultiAddress? ConnectedAddress { get; set; }

    /// <summary>
    /// Universally unique identifier.
    /// </summary>
    /// <value>This is the <see cref="MultiHash"/> of the peer's protobuf encoded <see cref="PublicKey"/>.</value>
    /// <seealso href="https://github.com/libp2p/specs/pull/100"/>
    public MultiHash? Id { get; set; }

    /// <summary>
    /// The round-trip time it takes to get data from the peer.
    /// </summary>
    public TimeSpan? Latency { get; set; }

    /// <summary>
    /// The name and version of the supported IPFS protocol.
    /// </summary>
    /// <value>For example "ipfs/0.1.0".</value>
    /// <remarks>
    /// There is no specification that describes the protocol version string. The default is "unknown/0.0".
    /// </remarks>
    public string ProtocolVersion { get; set; } = Unknown;

    /// <summary>
    /// The public key of the node.
    /// </summary>
    /// <value>The base 64 encoding of the node's public key. The default is <b>null</b></value>
    /// <remarks>
    /// The IPFS public key is the base-64 encoding of a protobuf encoding containing a type and
    /// the DER encoding of the PKCS Subject Public Key Info.
    /// </remarks>
    /// <seealso href="https://tools.ietf.org/html/rfc5280#section-4.1.2.7"/>
    public string? PublicKey { get; set; }

    /// <summary>
    /// Implicit casting of a <see cref="string"/> to a <see cref="Peer"/>.
    /// </summary>
    /// <param name="s">A <see cref="Base58"/> encoded <see cref="Id"/>.</param>
    /// <returns>A new <see cref="Peer"/>.</returns>
    /// <remarks>
    /// Equivalent to
    /// <code>new Peer { Id = s }</code>
    /// </remarks>
    public static implicit operator Peer(string s) => new() { Id = s };

    /// <summary>
    /// Value inequality.
    /// </summary>
    public static bool operator !=(Peer? a, Peer? b) => !(a == b);

    /// <summary>
    /// Value equality.
    /// </summary>
    public static bool operator ==(Peer? a, Peer? b) => object.ReferenceEquals(a, b) || (a is not null && b is not null && a.Equals(b));

    /// <inheritdoc/>
    public override bool Equals(object? obj) => (obj is Peer that) && Equals(that);

    /// <inheritdoc/>
    public bool Equals(Peer? that) => Id == that?.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => ToString().GetHashCode();

    /// <summary>
    /// Determines if the information on the peer is valid.
    /// </summary>
    /// <returns><b>true</b> if all validation rules pass; otherwise <b>false</b>.</returns>
    /// <remarks>
    /// Verifies that
    /// <list type="bullet">
    /// <item>
    /// <description>The <see cref="Id"/> is defined</description>
    /// </item>
    /// <item>
    /// <description>The <see cref="Id"/> is a hash of the <see cref="PublicKey"/></description>
    /// </item>
    /// </list>
    /// </remarks>
    public bool IsValid() => Id is not null && (PublicKey is null || Id.Matches(Convert.FromBase64String(PublicKey)));

    /// <summary>
    /// Converts this <see cref="Peer"/> to a <see cref="Peer.ToPeer"/>.
    /// </summary>
    /// <returns>A new <see cref="Peer.ToPeer"/>.</returns>
    public Peer ToPeer()
    {
        return new()
        {
            Addresses = Addresses,
            AgentVersion = AgentVersion,
            ConnectedAddress = ConnectedAddress,
            Id = Id,
            Latency = Latency,
            ProtocolVersion = ProtocolVersion,
            PublicKey = PublicKey,
        };
    }

    /// <summary>
    /// Returns the <see cref="Base58"/> encoding of the <see cref="Id"/>.
    /// </summary>
    /// <returns>A Base58 representaton of the peer.</returns>
    public override string ToString() => Id is null ? string.Empty : Id.ToBase58();
}
