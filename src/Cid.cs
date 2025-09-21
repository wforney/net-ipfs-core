using System.Text;
using Google.Protobuf;
using Newtonsoft.Json;

namespace Ipfs;

/// <summary>
///  Identifies some content, e.g. a Content ID.
/// </summary>
/// <remarks>
///   <para>
///   A Cid is a self-describing content-addressed identifier for distributed systems.
///   </para>
///   <para>
///   Initially, IPFS used a <see cref="MultiHash"/> as the CID and this is still supported as <see cref="Version"/> 0.
///   Version 1 adds a self describing structure to the multi-hash, see the <see href="https://github.com/ipld/cid">spec</see>.
///   </para>
///   <note>
///   The <see cref="MultiHash.Algorithm">hashing algorithm</see> must be "sha2-256" for a version 0 CID.
///   </note>
/// </remarks>
/// <seealso href="https://github.com/ipld/cid"/>
[JsonConverter(typeof(CidJsonConverter))]
public class Cid : IEquatable<Cid>
{
    /// <summary>
    ///   The default <see cref="ContentType"/>.
    /// </summary>
    public const string DefaultContentType = "dag-pb";

    private string? _encodedValue;
    private string _encoding = MultiBase.DefaultAlgorithmName;

    /// <summary>
    ///   Throws if a property cannot be set.
    /// </summary>
    /// <exception cref="NotSupportedException">
    ///   When a property cannot be set.
    /// </exception>
    /// <remarks>
    ///   Once <see cref="Encode"/> is invoked, the CID's properties
    ///   cannot be set.
    /// </remarks>
    private void EnsureMutable()
    {
        if (_encodedValue is not null)
        {
            throw new NotSupportedException("CID cannot be changed.");
        }
    }

    /// <summary>
    ///   The version of the CID.
    /// </summary>
    /// <value>
    ///   0 or 1.
    /// </value>
    /// <remarks>
    ///   <para>
    ///   When the <see cref="Version"/> is 0 and the following properties
    ///   are not matched, then the version is upgraded to version 1 when any
    ///   of the properties is set.
    ///   <list type="bullet">
    ///   <item><description><see cref="ContentType"/> equals "dag-pb"</description></item>
    ///   <item><description><see cref="Encoding"/> equals "base58btc"</description></item>
    ///   <item><description><see cref="Hash"/> algorithm name equals "sha2-256"</description></item>
    ///   </list>
    ///   </para>
    ///   <para>
    ///   </para>
    ///   The default <see cref="Encoding"/> is "base32" when the
    ///   <see cref="Version"/> is not zero.
    /// </remarks>
    public int Version
    {
        get;
        set
        {
            EnsureMutable();
            if (field == 0 && value > 0 && Encoding == "base58btc")
            {
                _encoding = "base32";
            }

            field = value;
        }
    }

    /// <summary>
    ///   The <see cref="MultiBase"/> encoding of the CID.
    /// </summary>
    /// <value>
    ///   base58btc, base32, base64, etc.  Defaults to <see cref="MultiBase.DefaultAlgorithmName"/>.
    /// </value>
    /// <remarks>
    ///    Sets <see cref="Version"/> to 1, when setting a value that
    ///    is not equal to "base58btc".
    /// </remarks>
    /// <seealso cref="MultiBase"/>
    public string Encoding
    {
        get => _encoding;
        set
        {
            EnsureMutable();
            if (Version == 0 && value != "base58btc")
            {
                Version = 1;
            }

            _encoding = value;
        }
    }

    /// <summary>
    ///   The content type or format of the data being addressed.
    /// </summary>
    /// <value>
    ///   dag-pb, dag-cbor, eth-block, etc.  Defaults to "dag-pb".
    /// </value>
    /// <remarks>
    ///    Sets <see cref="Version"/> to 1, when setting a value that
    ///    is not equal to "dag-pb".
    /// </remarks>
    /// <seealso cref="MultiCodec"/>
    public string ContentType
    {
        get;
        set
        {
            EnsureMutable();
            field = value;
            if (Version == 0 && value != "dag-pb")
            {
                Version = 1;
            }
        }
    } = DefaultContentType;

    /// <summary>
    ///   The cryptographic hash of the content being addressed.
    /// </summary>
    /// <value>
    ///   The <see cref="MultiHash"/> of the content being addressed.
    /// </value>
    /// <remarks>
    ///   Sets <see cref="Version"/> to 1, when setting a hashing algorithm that
    ///   is not equal to "sha2-256".
    ///   <note>
    ///   If the <see cref="MultiHash.Algorithm"/> equals <c>identity</c>, then
    ///   the <see cref="MultiHash.Digest"/> is also the content.  This is commonly
    ///   referred to as 'CID inlining'.
    ///   </note>
    /// </remarks>
    public MultiHash Hash
    {
        get => field ?? throw new InvalidDataException("Hash has not been set");
        set
        {
            EnsureMutable();
            field = value;
            if (Version == 0 && value?.Algorithm.Name != "sha2-256")
            {
                Version = 1;
            }
        }
    }

    /// <summary>
    ///   Returns the string representation of the CID in the
    ///   general format.
    /// </summary>
    /// <returns>
    ///  e.g. "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V"
    /// </returns>
    public override string ToString() => ToString("G");

    /// <summary>
    ///   Returns a string representation of the CID
    ///   according to the provided format specifier.
    /// </summary>
    /// <param name="format">
    ///   A single format specifier that indicates how to format the value of the
    ///   CID.  Can be "G" or "L".
    /// </param>
    /// <returns>
    ///   The CID in the specified <paramref name="format"/>.
    /// </returns>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is not valid.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   The "G" format specifier is the same as calling <see cref="Encode"/>.
    /// </para>
    /// <list type="table">
    /// <listheader>
    ///   <term>Specifier</term>
    ///   <description>return value</description>
    /// </listheader>
    ///  <item>
    ///    <term>G</term>
    ///    <description>QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V</description>
    ///  </item>
    ///  <item>
    ///    <term>L</term>
    ///    <description>base58btc cidv0 dag-pb sha2-256 Qm...</description>
    ///  </item>
    /// </list>
    /// </remarks>
    public string ToString(string format)
    {
        switch (format)
        {
            case "G":
                return Encode();

            case "L":
                var sb = new StringBuilder();
                _ = sb.Append(Encoding);
                _ = sb.Append(' ');
                _ = sb.Append("cidv");
                _ = sb.Append(Version);
                _ = sb.Append(' ');
                _ = sb.Append(ContentType);
                _ = sb.Append(' ');
                _ = sb.Append(Hash.Algorithm.Name);
                _ = sb.Append(' ');
                _ = sb.Append(MultiBase.Encode(Hash.ToArray(), Encoding)[1..]);
                return sb.ToString();

            default:
                throw new FormatException($"Invalid CID format specifier '{format}'.");
        }

    }

    /// <summary>
    ///   Converts the CID to its equivalent string representation.
    /// </summary>
    /// <returns>
    ///   The string representation of the <see cref="Cid"/>.
    /// </returns>
    /// <remarks>
    ///   For <see cref="Version"/> 0, this is equivalent to the
    ///   <see cref="MultiHash.ToBase58()">base58btc encoding</see>
    ///   of the <see cref="Hash"/>.
    /// </remarks>
    /// <seealso cref="Decode"/>
    public string Encode()
    {
        if (_encodedValue is not null)
        {
            return _encodedValue;
        }
        if (Version == 0)
        {
            _encodedValue = Hash.ToBase58();
        }
        else
        {
            using var ms = new MemoryStream();
            ms.WriteVarint(Version);
            ms.WriteMultiCodec(ContentType);
            Hash.Write(ms);
            _encodedValue = MultiBase.Encode(ms.ToArray(), Encoding);
        }
        return _encodedValue;
    }

    /// <summary>
    ///   Converts the specified <see cref="string"/>
    ///   to an equivalent <see cref="Cid"/> object.
    /// </summary>
    /// <param name="input">
    ///   The <see cref="Cid.Encode">CID encoded</see> string.
    /// </param>
    /// <returns>
    ///   A new <see cref="Cid"/> that is equivalent to <paramref name="input"/>.
    /// </returns>
    /// <exception cref="FormatException">
    ///   When the <paramref name="input"/> can not be decoded.
    /// </exception>
    /// <seealso cref="Encode"/>
    public static Cid Decode(string input)
    {
        try
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // SHA2-256 MultiHash is CID v0.
            if (input.Length == 46 && input.StartsWith("Qm", StringComparison.Ordinal))
            {
                return (Cid)new MultiHash(input);
            }

            using var ms = new MemoryStream(MultiBase.Decode(input), false);
            int v = ms.ReadVarint32();
            return v != 1
                ? throw new InvalidDataException($"Unknown CID version '{v}'.")
                : new Cid
                {
                    Version = v,
                    Encoding = Registry.MultiBaseAlgorithm.Codes[input[0]].Name,
                    ContentType = ms.ReadMultiCodec().Name,
                    Hash = new MultiHash(ms)
                };
        }
        catch (Exception e)
        {
            throw new FormatException($"Invalid CID '{input}'.", e);
        }
    }

    /// <summary>
    ///   Reads the binary representation of the CID from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">
    ///   The <see cref="Stream"/> to read from.
    /// </param>
    /// <returns>
    ///   A new <see cref="Cid"/>.
    /// </returns>
    public static Cid Read(Stream stream)
    {
        var cid = new Cid();
        int length = stream.ReadVarint32();
        if (length == 34)
        {
            cid.Version = 0;
        }
        else
        {
            cid.Version = stream.ReadVarint32();
            cid.ContentType = stream.ReadMultiCodec().Name;
        }
        cid.Hash = new MultiHash(stream);

        return cid;
    }

    /// <summary>
    ///   Writes the binary representation of the CID to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">
    ///   The <see cref="Stream"/> to write to.
    /// </param>
    public void Write(Stream stream)
    {
        using var ms = new MemoryStream();
        if (Version != 0)
        {
            ms.WriteVarint(Version);
            ms.WriteMultiCodec(ContentType);
        }
        Hash.Write(ms);

        stream.WriteVarint(ms.Length);
        ms.Position = 0;
        ms.CopyTo(stream);
    }

    /// <summary>
    ///   Reads the binary representation of the CID from the specified <see cref="CodedInputStream"/>.
    /// </summary>
    /// <param name="stream">
    ///   The <see cref="CodedInputStream"/> to read from.
    /// </param>
    /// <returns>
    ///   A new <see cref="Cid"/>.
    /// </returns>
    public static Cid Read(CodedInputStream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var cid = new Cid();
        int length = stream.ReadLength();
        if (length == 34)
        {
            cid.Version = 0;
        }
        else
        {
            cid.Version = stream.ReadInt32();
            cid.ContentType = stream.ReadMultiCodec().Name;
        }

        cid.Hash = new MultiHash(stream);

        return cid;
    }

    /// <summary>
    ///   Writes the binary representation of the CID to the specified <see cref="CodedOutputStream"/>.
    /// </summary>
    /// <param name="stream">
    ///   The <see cref="CodedOutputStream"/> to write to.
    /// </param>
    public void Write(CodedOutputStream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var ms = new MemoryStream();
        if (Version != 0)
        {
            ms.WriteVarint(Version);
            ms.WriteMultiCodec(ContentType);
        }
        Hash.Write(ms);

        byte[] bytes = ms.ToArray();
        stream.WriteLength(bytes.Length);
        stream.WriteSomeBytes(bytes);
    }

    /// <summary>
    ///   Reads the binary representation of the CID from the specified byte array.
    /// </summary>
    /// <param name="buffer">
    ///   The souce of a CID.
    /// </param>
    /// <returns>
    ///   A new <see cref="Cid"/>.
    /// </returns>
    /// <remarks>
    ///   The buffer does NOT start with a varint length prefix.
    /// </remarks>
    public static Cid Read(byte[] buffer)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var cid = new Cid();
        if (buffer?.Length == 34)
        {
            cid.Version = 0;
            cid.Hash = new MultiHash(buffer);
            return cid;
        }

        using var ms = new MemoryStream(buffer!, false);
        cid.Version = ms.ReadVarint32();
        cid.ContentType = ms.ReadMultiCodec().Name;
        cid.Hash = new MultiHash(ms);
        return cid;
    }

    /// <summary>
    ///   Returns the binary representation of the CID as a byte array.
    /// </summary>
    /// <returns>
    ///   A new buffer containing the CID.
    /// </returns>
    /// <remarks>
    ///   The buffer does NOT start with a varint length prefix.
    /// </remarks>
    public byte[] ToArray()
    {
        if (Version == 0)
        {
            return Hash.ToArray();
        }

        using var ms = new MemoryStream();
        ms.WriteVarint(Version);
        ms.WriteMultiCodec(ContentType);
        Hash.Write(ms);
        return ms.ToArray();
    }

    /// <summary>
    ///   Implicit casting of a <see cref="MultiHash"/> to a <see cref="Cid"/>.
    /// </summary>
    /// <param name="hash">
    ///   A <see cref="MultiHash"/>.
    /// </param>
    /// <returns>
    ///   A new <see cref="Cid"/> based on the <paramref name="hash"/>.  A <see cref="Version"/> 0
    ///   CID is returned if the <paramref name="hash"/> is "sha2-356"; otherwise <see cref="Version"/> 1.
    /// </returns>
    public static implicit operator Cid(MultiHash hash)
    {
        return hash?.Algorithm.Name == "sha2-256"
            ? new Cid
            {
                Hash = hash,
                Version = 0,
                Encoding = "base58btc",
                ContentType = "dag-pb"
            }
            : new Cid
            {
                Version = 1,
                Hash = hash!
            };
    }
    /// <inheritdoc />
    public override int GetHashCode() => Encode().GetHashCode();

    /// <inheritdoc />
    public override bool Equals(object? obj) => (obj is Cid that) && Encode() == that.Encode();

    /// <inheritdoc />
    public bool Equals(Cid? that) => Encode() == that?.Encode();

    /// <summary>
    ///   Value equality.
    /// </summary>
    public static bool operator ==(Cid? a, Cid? b) => ReferenceEquals(a, b) || (a is not null && b is not null && a.Equals(b));

    /// <summary>
    ///   Value inequality.
    /// </summary>
    public static bool operator !=(Cid? a, Cid? b) => !(a == b);

    /// <summary>
    ///   Implicit casting of a <see cref="string"/> to a <see cref="Cid"/>.
    /// </summary>
    /// <param name="s">
    ///   A string encoded <b>Cid</b>.
    /// </param>
    /// <returns>
    ///   A new <see cref="Cid"/>.
    /// </returns>
    /// <remarks>
    ///    Equivalent to <code> Cid.Decode(s)</code>
    /// </remarks>
    public static implicit operator Cid(string s) => Cid.Decode(s);

    /// <summary>
    ///   Implicit casting of a <see cref="Cid"/> to a <see cref="string"/>.
    /// </summary>
    /// <param name="id">
    ///   A <b>Cid</b>.
    /// </param>
    /// <returns>
    ///   A new <see cref="string"/>.
    /// </returns>
    /// <remarks>
    ///    Equivalent to <code>Cid.Encode()</code>
    /// </remarks>
    public static implicit operator string(Cid id) => id.Encode();
}

/// <summary>
///   Conversion of a <see cref="Cid"/> to and from JSON.
/// </summary>
/// <remarks>
///   The JSON is just a single string value.
/// </remarks>
public class CidJsonConverter : JsonConverter
{
    /// <inheritdoc />
    public override bool CanConvert(Type objectType) => true;

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanWrite => true;

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        var cid = value as Cid;
        writer.WriteValue(cid?.Encode());
    }

    /// <inheritdoc />
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
        reader is null ? throw new ArgumentNullException(nameof(reader)) : (object?)(reader.Value is string s ? Cid.Decode(s) : null);
}
