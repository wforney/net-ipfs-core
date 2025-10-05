using System.Runtime.Serialization;
using Google.Protobuf;

namespace Ipfs;

/// <summary>
///   A node in the IPFS Merkle DAG.
/// </summary>
/// <remarks>
///   A <b>DagNode</b> has opaque <see cref="DagNode.DataBytes"/>
///   and a set of navigable <see cref="DagNode.ReadOnlyLinks"/>.
/// </remarks>
[DataContract]
public sealed class DagNode : IMerkleNode<IMerkleLink>
{
    private string _hashAlgorithm = MultiHash.DefaultAlgorithmName;
    private ulong? _size;

    /// <summary>
    ///   Create a new instance of a <see cref="DagNode"/> with the specified
    ///   <see cref="DagNode.DataBytes"/> and <see cref="DagNode.ReadOnlyLinks"/>
    /// </summary>
    /// <param name="data">
    ///   The opaque data, can be <b>null</b>.
    /// </param>
    /// <param name="links">
    ///   The links to other nodes.
    /// </param>
    /// <param name="hashAlgorithm">
    ///   The name of the hashing algorithm to use; defaults to
    ///   <see cref="MultiHash.DefaultAlgorithmName"/>.
    /// </param>
    public DagNode(ReadOnlyMemory<byte>? data, IEnumerable<IMerkleLink>? links = null, string hashAlgorithm = MultiHash.DefaultAlgorithmName)
    {
        DataBytes = data ?? ReadOnlyMemory<byte>.Empty;
        var orderedLinks = (links ?? Array.Empty<IMerkleLink>())
            .OrderBy(link => link.Name ?? "")
            .ToList()
            .AsReadOnly();
        ReadOnlyLinks = orderedLinks;
        _hashAlgorithm = hashAlgorithm;
    }

    /// <summary>
    ///   Creates a new instance of the <see cref="DagNode"/> class from the
    ///   specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">
    ///   A <see cref="Stream"/> containing the binary representation of the
    ///   <b>DagNode</b>.
    /// </param>
    public DagNode(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        Read(stream);
    }

    /// <summary>
    ///   Creates a new instance of the <see cref="DagNode"/> class from the
    ///   specified <see cref="CodedInputStream"/>.
    /// </summary>
    /// <param name="stream">(
    ///   A <see cref="CodedInputStream"/> containing the binary representation of the
    ///   <b>DagNode</b>.
    /// </param>
    public DagNode(CodedInputStream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        Read(stream);
    }

    /// <inheritdoc />
    [DataMember]
    public IReadOnlyCollection<IMerkleLink> ReadOnlyLinks { get; private set; } = Array.Empty<IMerkleLink>();
    IEnumerable<IMerkleLink> IMerkleNode<IMerkleLink>.Links => ReadOnlyLinks;

    /// <inheritdoc />
    [DataMember]
    public ReadOnlyMemory<byte> DataBytes { get; private set; } = ReadOnlyMemory<byte>.Empty;

    /// <inheritdoc />
    public Stream DataStream => new MemoryStream(DataBytes.ToArray(), false);

    /// <summary>
    ///   The serialised size in bytes of the node.
    /// </summary>
    [DataMember]
    public ulong Size
    {
        get
        {
            _size ??= ComputeSize();
            return _size.Value;
        }
    }

    /// <inheritdoc />
    [DataMember]
    public Cid Id
    {
        get
        {
            field ??= ComputeHash();
            return field;
        }
        set
        {
            field = value;
            _hashAlgorithm = field.Hash.Algorithm.Name;
        }
    }

    /// <inheritdoc />
    public IMerkleLink ToLink(string name = "") => new DagLink(name, Id, Size);

    /// <summary>
    ///   Adds a link.
    /// </summary>
    /// <param name="link">
    ///   The link to add.
    /// </param>
    /// <returns>
    ///   A new <see cref="DagNode"/> with the existing and new
    ///   links.
    /// </returns>
    /// <remarks>
    ///   A <b>DagNode</b> is immutable.
    /// </remarks>
    public DagNode AddLink(IMerkleLink link) => AddLinks([link]);

    /// <summary>
    ///   Adds a sequence of links.
    /// </summary>
    /// <param name="links">
    ///   The sequence of links to add.
    /// </param>
    /// <returns>
    ///   A new <see cref="DagNode"/> with the existing and new
    ///   links.
    /// </returns>
    /// <remarks>
    ///   A <b>DagNode</b> is immutable.
    /// </remarks>
    public DagNode AddLinks(IEnumerable<IMerkleLink> links)
    {
        IReadOnlyCollection<IMerkleLink> all = ReadOnlyLinks.Union(links).ToList().AsReadOnly();
        return new DagNode(DataBytes, all, _hashAlgorithm);
    }

    /// <summary>
    ///   Removes a link.
    /// </summary>
    /// <param name="link">
    ///   The <see cref="IMerkleLink"/> to remove.
    /// </param>
    /// <returns>
    ///   A new <see cref="DagNode"/> with the <paramref name="link"/>
    ///   removed.
    /// </returns>
    /// <remarks>
    ///   A <b>DagNode</b> is immutable.
    ///   <para>
    ///   No exception is raised if the <paramref name="link"/> does
    ///   not exist.
    ///   </para>
    /// </remarks>
    public DagNode RemoveLink(IMerkleLink link) => RemoveLinks([link]);

    /// <summary>
    ///   Remove a sequence of links.
    /// </summary>
    /// <param name="links">
    ///   The sequence of <see cref="IMerkleLink"/> to remove.
    /// </param>
    /// <returns>
    ///   A new <see cref="DagNode"/> with the <paramref name="links"/>
    ///   removed.
    /// </returns>
    /// <remarks>
    ///   A <b>DagNode</b> is immutable.
    ///   <para>
    ///   No exception is raised if any of the <paramref name="links"/> do
    ///   not exist.
    ///   </para>
    /// </remarks>
    public DagNode RemoveLinks(IEnumerable<IMerkleLink> links)
    {
        ILookup<Cid, IMerkleLink> ignore = links.ToLookup(link => link.Id);
        IReadOnlyCollection<IMerkleLink> some = ReadOnlyLinks.Where(link => !ignore.Contains(link.Id)).ToList().AsReadOnly();
        return new DagNode(DataBytes, some, _hashAlgorithm);
    }

    /// <summary>
    ///   Writes the binary representation of the node to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">
    ///   The <see cref="Stream"/> to write to.
    /// </param>
    public void Write(Stream stream)
    {
        using var cos = new CodedOutputStream(stream, true);
        Write(cos);
    }

    /// <summary>
    ///   Writes the binary representation of the node to the specified <see cref="CodedOutputStream"/>.
    /// </summary>
    /// <param name="stream">
    ///   The <see cref="CodedOutputStream"/> to write to.
    /// </param>
    public void Write(CodedOutputStream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        foreach (DagLink? link in ReadOnlyLinks.Select(l => new DagLink(l)))
        {
            using var linkStream = new MemoryStream();
            link.Write(linkStream);
            byte[] msg = linkStream.ToArray();
            stream.WriteTag(2, WireFormat.WireType.LengthDelimited);
            stream.WriteLength(msg.Length);
            stream.WriteSomeBytes(msg);
        }

        if (!DataBytes.IsEmpty)
        {
            stream.WriteTag(1, WireFormat.WireType.LengthDelimited);
            stream.WriteLength(DataBytes.Length);
            stream.WriteSomeBytes(DataBytes.ToArray());
        }
    }

    private void Read(Stream stream)
    {
        using var cis = new CodedInputStream(stream, true);
        Read(cis);
    }

    private void Read(CodedInputStream stream)
    {
        var links = new List<DagLink>();
        bool done = false;

        while (!stream.IsAtEnd && !done)
        {
            uint tag = stream.ReadTag();
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1:
                    DataBytes = new ReadOnlyMemory<byte>(stream.ReadSomeBytes(stream.ReadLength())!);
                    done = true;
                    break;
                case 2:
                    using (var ms = new MemoryStream(stream.ReadSomeBytes(stream.ReadLength())!))
                    {
                        links.Add(new DagLink(ms));
                    }
                    break;
                default:
                    throw new InvalidDataException("Unknown field number");
            }
        }

        DataBytes = DataBytes.IsEmpty ? ReadOnlyMemory<byte>.Empty : DataBytes;
        ReadOnlyLinks = links.AsReadOnly();
    }

    /// <summary>
    ///   Returns the IPFS binary representation as a byte array.
    /// </summary>
    /// <returns>
    ///   A byte array.
    /// </returns>
    public byte[] ToArray()
    {
        using var ms = new MemoryStream();
        Write(ms);
        return ms.ToArray();
    }

    private MultiHash ComputeHash()
    {
        using var ms = new MemoryStream();
        Write(ms);
        _size = (ulong?)ms.Position;
        ms.Position = 0;
        return MultiHash.ComputeHash(ms, _hashAlgorithm);
    }

    private ulong ComputeSize()
    {
        using var ms = new MemoryStream();
        Write(ms);
        return (ulong)ms.Position;
    }
}
