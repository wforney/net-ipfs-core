using Newtonsoft.Json;

namespace Ipfs;

/// <summary>
/// A wrapper for a <see cref="Cid"/> that is used in an IPLD Directed Acyclic Graph (DAG).
/// </summary>
/// <remarks>
/// See also <see href="https://ipld.io/specs/codecs/dag-json/spec/#links"/>.
/// </remarks>
public record DagCid
{
    /// <summary>
    /// Converts a <see cref="DagCid"/> to a <see cref="Cid"/>.
    /// </summary>
    /// <param name="dagLink">The <see cref="DagCid"/> to convert.</param>
    /// <returns>The <see cref="Cid"/> value.</returns>
    public static Cid ToCid(DagCid dagLink) => dagLink is null ? null! : dagLink.Value;

    /// <summary>
    /// Converts a <see cref="Cid"/> to a <see cref="DagCid"/>.
    /// </summary>
    /// <param name="cid">The <see cref="Cid"/> to convert.</param>
    /// <returns>The <see cref="DagCid"/> value.</returns>
    public static DagCid ToDagCid(Cid cid) => cid is null || cid.ContentType == "libp2p-key" ? null! : new DagCid { Value = cid };

    /// <summary>
    /// The <see cref="Cid"/> value of this DAG link.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when attempting to set a CID with ContentType "libp2p-key",
    /// as IPLD links must be immutable and libp2p-key CIDs represent mutable IPNS addresses.
    /// </exception>
    [JsonProperty("/")]
    public required Cid Value
    {
        get;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.ContentType == "libp2p-key")
            {
                throw new ArgumentException(
                    "Cannot store CID-encoded libp2p key as DagCid link. " +
                    "IPLD links must be immutable, but libp2p-key CIDs represent mutable IPNS addresses. " +
                    "Use the resolved content CID instead.",
                    nameof(value));
            }
            field = value;
        }
    } = null!;

    /// <summary>
    /// Implicit casting of a <see cref="DagCid"/> to a <see cref="Cid"/>.
    /// </summary>
    /// <param name="dagLink">The <see cref="DagCid"/> to cast.</param>
    public static implicit operator Cid(DagCid dagLink) => ToCid(dagLink);

    /// <summary>
    /// Explicit casting of a <see cref="Cid"/> to a <see cref="DagCid"/>.
    /// </summary>
    /// <param name="cid">The <see cref="Cid"/> to cast.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when attempting to cast a CID with ContentType "libp2p-key",
    /// as IPLD links must be immutable and libp2p-key CIDs represent mutable IPNS addresses.
    /// </exception>
    public static explicit operator DagCid(Cid cid) => ToDagCid(cid);

    /// <summary>
    /// Returns the string representation of the <see cref="DagCid"/>.
    /// </summary>
    /// <returns>
    ///  e.g. "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V"
    /// </returns>
    public override string ToString() => Value.ToString();
}
