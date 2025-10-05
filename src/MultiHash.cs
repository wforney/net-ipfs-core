using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Google.Protobuf;
using Ipfs.Registry;
using Newtonsoft.Json;

namespace Ipfs;

/// <summary>
/// A protocol for differentiating outputs from various well-established cryptographic hash
/// functions, addressing size + encoding considerations.
/// </summary>
/// <remarks>See the <see cref="HashingAlgorithm">registry</see> for supported algorithms.</remarks>
/// <seealso href="https://github.com/jbenet/multihash"/>
[JsonConverter(typeof(MultiHash.Json))]
public class MultiHash : IEquatable<MultiHash>
{
    /// <summary>
    /// The default hashing algorithm is "sha2-256".
    /// </summary>
    public const string DefaultAlgorithmName = "sha2-256";

    /// <summary>
    /// The cached base-58 encoding of the multihash.
    /// </summary>
    private string? _b58String;

    /// <summary>
    /// Creates a new instance of the <see cref="MultiHash"/> class with the specified <see
    /// cref="HashingAlgorithm">Algorithm name</see> and <see cref="Digest"/> value.
    /// </summary>
    /// <param name="algorithmName">A valid IPFS hashing algorithm name, e.g. "sha2-256" or "sha2-512".</param>
    /// <param name="digest">The digest value as a byte array.</param>
    public MultiHash(string algorithmName, byte[] digest)
    {
        if (!HashingAlgorithm.Names.TryGetValue(algorithmName, out HashingAlgorithm? a))
        {
            throw new ArgumentException($"The IPFS hashing algorithm '{algorithmName}' is unknown.");
        }

        Algorithm = a;

        if (Algorithm.DigestSize != 0 && Algorithm.DigestSize != digest?.Length)
        {
            throw new ArgumentException($"The digest size for '{algorithmName}' is {Algorithm.DigestSize} bytes, not {digest?.Length}.");
        }

        Digest = digest;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MultiHash"/> class from the specified byte array.
    /// </summary>
    /// <param name="buffer">A sequence of bytes containing the binary representation of the <b>MultiHash</b>.</param>
    /// <remarks>
    /// Reads the binary representation of <see cref="MultiHash"/> from the <paramref name="buffer"/>.
    /// <para>
    /// The binary representation is a <see cref="Varint"/> of the <see
    /// cref="HashingAlgorithm.Code"/>, <see cref="Varint"/> of the <see
    /// cref="HashingAlgorithm.DigestSize"/> followed by the <see cref="Digest"/>.
    /// </para>
    /// <para>
    /// When an unknown <see cref="HashingAlgorithm.Code">hashing algorithm number</see> is
    /// encountered a new hashing algorithm is <see
    /// cref="HashingAlgorithm.Register">registered</see>. This new algorithm does not support
    /// matching nor computing a hash. This behaviour allows parsing of any well formed <see
    /// cref="MultiHash"/> even when the hashing algorithm is unknown.
    /// </para>
    /// </remarks>
    /// <seealso cref="ToArray"/>
    public MultiHash(byte[] buffer)
    {
        using var ms = new MemoryStream(buffer, false);
        (Algorithm, Digest) = Read(ms);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MultiHash"/> class from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">
    /// A <see cref="Stream"/> containing the binary representation of the <b>MultiHash</b>.
    /// </param>
    /// <remarks>
    /// Reads the binary representation of <see cref="MultiHash"/> from the <paramref name="stream"/>.
    /// <para>
    /// The binary representation is a <see cref="Varint"/> of the <see
    /// cref="HashingAlgorithm.Code"/>, <see cref="Varint"/> of the <see
    /// cref="HashingAlgorithm.DigestSize"/> followed by the <see cref="Digest"/>.
    /// </para>
    /// <para>
    /// When an unknown <see cref="HashingAlgorithm.Code">hashing algorithm number</see> is
    /// encountered a new hashing algorithm is <see
    /// cref="HashingAlgorithm.Register">registered</see>. This new algorithm does not support
    /// matching nor computing a hash. This behaviour allows parsing of any well formed <see
    /// cref="MultiHash"/> even when the hashing algorithm is unknown.
    /// </para>
    /// </remarks>
    public MultiHash(Stream stream) => (Algorithm, Digest) = Read(stream);

    /// <summary>
    /// Creates a new instance of the <see cref="MultiHash"/> class from the specified <see cref="CodedInputStream"/>.
    /// </summary>
    /// <param name="stream">
    /// A <see cref="CodedInputStream"/> containing the binary representation of the <b>MultiHash</b>.
    /// </param>
    /// <remarks>
    /// Reads the binary representation of <see cref="MultiHash"/> from the <paramref name="stream"/>.
    /// <para>
    /// The binary representation is a <see cref="Varint"/> of the <see
    /// cref="HashingAlgorithm.Code"/>, <see cref="Varint"/> of the <see
    /// cref="HashingAlgorithm.DigestSize"/> followed by the <see cref="Digest"/>.
    /// </para>
    /// <para>
    /// When an unknown <see cref="HashingAlgorithm.Code">hashing algorithm number</see> is
    /// encountered a new hashing algorithm is <see
    /// cref="HashingAlgorithm.Register">registered</see>. This new algorithm does not support
    /// matching nor computing a hash. This behaviour allows parsing of any well formed <see
    /// cref="MultiHash"/> even when the hashing algorithm is unknown.
    /// </para>
    /// </remarks>
    public MultiHash(CodedInputStream stream)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(stream);
#else
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
#endif

        (Algorithm, Digest) = Read(stream);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MultiHash"/> class from the specified <see
    /// cref="Base58"/> encoded <see cref="string"/>.
    /// </summary>
    /// <param name="s">A <see cref="Base58"/> encoded <b>MultiHash</b>.</param>
    /// <remarks>
    /// <para>
    /// When an unknown <see cref="HashingAlgorithm.Code">hashing algorithm number</see> is
    /// encountered a new hashing algorithm is <see
    /// cref="HashingAlgorithm.Register">registered</see>. This new algorithm does not support
    /// matching nor computing a hash. This behaviour allows parsing of any well formed <see
    /// cref="MultiHash"/> even when the hashing algorithm is unknown.
    /// </para>
    /// </remarks>
    /// <seealso cref="ToBase58"/>
    public MultiHash(string s)
    {
        using var ms = new MemoryStream(s.FromBase58(), false);
        (Algorithm, Digest) = Read(ms);
    }

    /// <summary>
    /// Occurs when an unknown hashing algorithm number is parsed.
    /// </summary>
    public static event EventHandler<UnknownHashingAlgorithmEventArgs>? UnknownHashingAlgorithm;

    /// <summary>
    /// The hashing algorithm.
    /// </summary>
    /// <value>Details on the hashing algorithm.</value>
    public HashingAlgorithm Algorithm { get; }

    /// <summary>
    /// The hashing algorithm's digest value.
    /// </summary>
    /// <value>The output of the hashing algorithm.</value>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Public API")]
    public byte[] Digest { get; }

    /// <summary>
    /// Determines if the identity hash algorithm is in use.
    /// </summary>
    /// <value><b>true</b> if the identity hash algorithm is used; otherwise, <b>false</b>.</value>
    /// <remarks>
    /// The identity hash is used to inline a small amount of data into a <see cref="Cid"/>. When
    /// <b>true</b>, the <see cref="Digest"/> is also the content.
    /// </remarks>
    public bool IsIdentityHash => Algorithm.Code == 0;

    /// <summary>
    /// Generate the multihash for the specified byte array.
    /// </summary>
    /// <param name="data">The byte array containing the data to hash.</param>
    /// <param name="algorithmName">
    /// The name of the hashing algorithm to use; defaults to <see cref="DefaultAlgorithmName"/>.
    /// </param>
    /// <returns>A <see cref="MultiHash"/> for the <paramref name="data"/>.</returns>
    public static MultiHash ComputeHash(byte[] data, string algorithmName = DefaultAlgorithmName)
    {
        using HashAlgorithm alg = GetHashAlgorithm(algorithmName);
        return new MultiHash(algorithmName, alg.ComputeHash(data));
    }

    /// <summary>
    /// Generate the multihash for the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="data">The <see cref="Stream"/> containing the data to hash.</param>
    /// <param name="algorithmName">
    /// The name of the hashing algorithm to use; defaults to <see cref="DefaultAlgorithmName"/>.
    /// </param>
    /// <returns>A <see cref="MultiHash"/> for the <paramref name="data"/>.</returns>
    public static MultiHash ComputeHash(Stream data, string algorithmName = DefaultAlgorithmName)
    {
        using HashAlgorithm alg = GetHashAlgorithm(algorithmName);
        return new MultiHash(algorithmName, alg.ComputeHash(data));
    }

    /// <summary>
    /// Gets the <see cref="HashAlgorithm"/> with the specified IPFS multi-hash name.
    /// </summary>
    /// <param name="name">
    /// The name of a hashing algorithm, see <see
    /// href="https://github.com/multiformats/multicodec/blob/master/table.csv"/> for IPFS defined names.
    /// </param>
    /// <returns>
    /// The hashing implementation associated with the <paramref name="name"/>. After using the
    /// hashing algorithm it should be disposed.
    /// </returns>
    /// <exception cref="KeyNotFoundException">When <paramref name="name"/> is not registered.</exception>
    public static HashAlgorithm GetHashAlgorithm(string name = DefaultAlgorithmName)
    {
        return HashingAlgorithm.Names.TryGetValue(name, out HashingAlgorithm? hashAlgorithm)
            ? hashAlgorithm.Hasher()
            : throw new KeyNotFoundException($"Hash algorithm '{name}' is not registered.");
    }

    /// <summary>
    /// Gets the name of hashing algorithm name with the specified code.
    /// </summary>
    /// <param name="code">
    /// The code of a hashing algorithm, see <see
    /// href="https://github.com/multiformats/multicodec/blob/master/table.csv"/> for IPFS defined codes.
    /// </param>
    /// <returns>The name assigned to <paramref name="code"/>.</returns>
    /// <exception cref="KeyNotFoundException">When <paramref name="code"/> is not registered.</exception>
    public static string GetHashAlgorithmName(int code)
    {
        return HashingAlgorithm.Codes.TryGetValue(code, out HashingAlgorithm? hashingAlgorithm)
            ? hashingAlgorithm.Name
            : throw new KeyNotFoundException($"Hash algorithm with code '{code}' is not registered.");
    }

    /// <summary>
    /// Implicit casting of a <see cref="string"/> to a <see cref="MultiHash"/>.
    /// </summary>
    /// <param name="s">A <see cref="Base58"/> encoded <b>MultiHash</b>.</param>
    /// <returns>A new <see cref="MultiHash"/>.</returns>
    /// <remarks>
    /// Equivalent to
    /// <code>new MultiHash(s)</code>
    /// </remarks>
    public static implicit operator MultiHash(string s) => new(s);

    /// <summary>
    /// Value inequality.
    /// </summary>
    public static bool operator !=(MultiHash? a, MultiHash? b) => !(a == b);

    /// <summary>
    /// Value equality.
    /// </summary>
    public static bool operator ==(MultiHash? a, MultiHash? b) => ReferenceEquals(a, b) || (a is not null && b is not null && a.Equals(b));

    /// <inheritdoc/>
    public override bool Equals(object? obj) => (obj is MultiHash that) && Equals(that);

    /// <inheritdoc/>
    public bool Equals(MultiHash? that) => Algorithm.Code == that?.Algorithm.Code && Digest.SequenceEqual(that.Digest);

    /// <inheritdoc/>
    public override int GetHashCode() => ToString().GetHashCode();

    /// <summary>
    /// Determines if the data matches the hash.
    /// </summary>
    /// <param name="data">The data to check.</param>
    /// <returns><b>true</b> if the data matches the <see cref="MultiHash"/>; otherwise, <b>false</b>.</returns>
    /// <remarks><b>Matches</b> is used to ensure data integrity.</remarks>
    public bool Matches(byte[] data)
    {
        byte[] digest = Algorithm.Hasher().ComputeHash(data);
        for (int i = digest.Length - 1; 0 <= i; --i)
        {
            if (digest[i] != Digest[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines if the stream data matches the hash.
    /// </summary>
    /// <param name="data">The <see cref="Stream"/> containing the data to check.</param>
    /// <returns><b>true</b> if the data matches the <see cref="MultiHash"/>; otherwise, <b>false</b>.</returns>
    /// <remarks><b>Matches</b> is used to ensure data integrity.</remarks>
    public bool Matches(Stream data)
    {
        byte[] digest = Algorithm.Hasher().ComputeHash(data);
        for (int i = digest.Length - 1; 0 <= i; --i)
        {
            if (digest[i] != Digest[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the IPFS binary representation as a byte array.
    /// </summary>
    /// <returns>A byte array.</returns>
    /// <remarks>The binary representation is a sequence of <see cref="MultiHash"/>.</remarks>
    public byte[] ToArray()
    {
        using var ms = new MemoryStream();
        Write(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Returns the <see cref="Base32"/> encoding of the <see cref="MultiHash"/>.
    /// </summary>
    /// <returns>The <see cref="Base32"/> representation of the <see cref="MultiHash"/>.</returns>
    public string ToBase32() => ToArray().ToBase32();

    /// <summary>
    /// Returns the <see cref="Base58"/> encoding of the <see cref="MultiHash"/>.
    /// </summary>
    /// <returns>The <see cref="Base58"/> representation of the <see cref="MultiHash"/>.</returns>
    public string ToBase58()
    {
        if (_b58String is not null)
        {
            return _b58String;
        }

        using var ms = new MemoryStream();
        Write(ms);
        _b58String = ms.ToArray().ToBase58();
        return _b58String;
    }

    /// <summary>
    /// Converts the <see cref="MultiHash"/> to a new <see cref="MultiHash"/>.
    /// </summary>
    /// <returns>
    /// A new <see cref="MultiHash"/> with the same <see cref="Algorithm"/> and <see cref="Digest"/>.
    /// </returns>
    public MultiHash ToMultiHash() => new(Algorithm.ToString(), Digest);

    /// <summary>
    /// Returns the <see cref="Base58"/> encoding of the <see cref="MultiHash"/>.
    /// </summary>
    /// <returns>A base-58 representaton of the MultiHash.</returns>
    /// <seealso cref="ToBase58"/>
    public override string ToString() => ToBase58();

    /// <summary>
    /// Writes the binary representation of the multihash to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to write to.</param>
    /// <remarks>
    /// The binary representation is a 1-byte <see cref="HashingAlgorithm.Code"/>, 1-byte <see
    /// cref="HashingAlgorithm.DigestSize"/> followed by the <see cref="Digest"/>.
    /// </remarks>
    public void Write(Stream stream)
    {
        using var cos = new CodedOutputStream(stream, true);
        Write(cos);
    }

    /// <summary>
    /// Writes the binary representation of the multihash to the specified <see cref="CodedOutputStream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="CodedOutputStream"/> to write to.</param>
    /// <remarks>
    /// The binary representation is a <see cref="Varint"/> of the <see
    /// cref="HashingAlgorithm.Code"/>, <see cref="Varint"/> of the <see
    /// cref="HashingAlgorithm.DigestSize"/> followed by the <see cref="Digest"/>.
    /// </remarks>
    public void Write(CodedOutputStream stream)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(stream);
#else
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
#endif

        stream.WriteInt32(Algorithm.Code);
        stream.WriteLength(Digest.Length);
        stream.WriteSomeBytes(Digest);
    }

    private void RaiseUnknownHashingAlgorithm(HashingAlgorithm algorithm)
    {
        EventHandler<UnknownHashingAlgorithmEventArgs>? handler = UnknownHashingAlgorithm;
        if (handler is not null)
        {
            var args = new UnknownHashingAlgorithmEventArgs(algorithm);
            handler(this, args);
        }
    }

    private (HashingAlgorithm, byte[]) Read(Stream stream)
    {
        using var cis = new CodedInputStream(stream, true);
        return Read(cis);
    }

    private (HashingAlgorithm, byte[]) Read(CodedInputStream stream)
    {
        int code = stream.ReadInt32();
        int digestSize = stream.ReadLength();

        _ = HashingAlgorithm.Codes.TryGetValue(code, out HashingAlgorithm? a);
        if (a is null)
        {
            a = HashingAlgorithm.Register("ipfs-" + code, code, digestSize);
            RaiseUnknownHashingAlgorithm(a);
        }
        else if (a.DigestSize != 0 && digestSize != a.DigestSize)
        {
            throw new InvalidDataException($"The digest size {digestSize} is wrong for {a.Name}; it should be {a.DigestSize}.");
        }

        byte[] digest = stream.ReadSomeBytes(digestSize)!;
        return (a, digest);
    }

    /// <summary>
    /// Conversion of a <see cref="MultiHash"/> to and from JSON.
    /// </summary>
    /// <remarks>The JSON is just a single string value.</remarks>
    private class Json : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => true;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => reader.Value is string s ? new MultiHash(s) : null;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var mh = value as MultiHash;
            writer.WriteValue(mh?.ToString());
        }
    }
}
