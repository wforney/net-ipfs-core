namespace Ipfs.Registry;

/// <summary>
///   Metadata and implementations of an IPFS multi-base algorithm.
/// </summary>
/// <remarks>
///   IPFS assigns a unique <see cref="Name"/> and <see cref="Code"/> to multi-base algorithm.
///   See <see href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/> for
///   the currently defined multi-base algorithms.
///   <para>
///   These algorithms are supported: base58btc, base58flickr, base64,
///   base64pad, base64url, base16, base32, base32z, base32pad, base32hex
///   and base32hexpad.
///   </para>
/// </remarks>
public class MultiBaseAlgorithm
{
    internal static Dictionary<string, MultiBaseAlgorithm> Names = new(StringComparer.Ordinal);
    internal static Dictionary<char, MultiBaseAlgorithm> Codes = [];

    /// <summary>
    ///   Register the standard multi-base algorithms for IPFS.
    /// </summary>
    /// <seealso href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/>
    static MultiBaseAlgorithm()
    {
        _ = Register("base58btc", 'z', static bytes => SimpleBase.Base58.Bitcoin.Encode(bytes), static s => ToByteArray(SimpleBase.Base58.Bitcoin.Decode(s)));
        _ = Register("base58flickr", 'Z', static bytes => SimpleBase.Base58.Flickr.Encode(bytes), static s => ToByteArray(SimpleBase.Base58.Flickr.Decode(s)));
        _ = Register("base64", 'm', static bytes => bytes.ToBase64NoPad(), static s => s.FromBase64NoPad());
        _ = Register("base64pad", 'M', static bytes => Convert.ToBase64String(bytes), static s => Convert.FromBase64String(s));
        _ = Register("base64url", 'u', static bytes => bytes.ToBase64Url(), static s => s.FromBase64Url());
        _ = Register("base16", 'f', static bytes => HexLower(bytes), static s => ToByteArray(SimpleBase.Base16.Decode(s)));
        _ = Register("base32", 'b', static bytes => SimpleBase.Base32.Rfc4648.Encode(bytes, false).ToLowerInvariant(), static s => ToByteArray(SimpleBase.Base32.Rfc4648.Decode(s)));
        _ = Register("base32pad", 'c', static bytes => SimpleBase.Base32.Rfc4648.Encode(bytes, true).ToLowerInvariant(), static s => ToByteArray(SimpleBase.Base32.Rfc4648.Decode(s)));
        _ = Register("base32hex", 'v', static bytes => SimpleBase.Base32.ExtendedHex.Encode(bytes, false).ToLowerInvariant(), static s => ToByteArray(SimpleBase.Base32.ExtendedHex.Decode(s)));
        _ = Register("base32hexpad", 't', static bytes => SimpleBase.Base32.ExtendedHex.Encode(bytes, true).ToLowerInvariant(), static s => ToByteArray(SimpleBase.Base32.ExtendedHex.Decode(s)));
        _ = Register("base36", 'k', Base36.EncodeToStringLc, Base36.DecodeString);
        _ = Register("BASE16", 'F', static bytes => HexUpper(bytes), static s => ToByteArray(SimpleBase.Base16.Decode(s)));
        _ = Register("BASE32", 'B', static bytes => SimpleBase.Base32.Rfc4648.Encode(bytes, false), static s => ToByteArray(SimpleBase.Base32.Rfc4648.Decode(s)));
        _ = Register("BASE32PAD", 'C', static bytes => SimpleBase.Base32.Rfc4648.Encode(bytes, true), static s => ToByteArray(SimpleBase.Base32.Rfc4648.Decode(s)));
        _ = Register("BASE32HEX", 'V', static bytes => SimpleBase.Base32.ExtendedHex.Encode(bytes, false), static s => ToByteArray(SimpleBase.Base32.ExtendedHex.Decode(s)));
        _ = Register("BASE32HEXPAD", 'T', static bytes => SimpleBase.Base32.ExtendedHex.Encode(bytes, true), static s => ToByteArray(SimpleBase.Base32.ExtendedHex.Decode(s)));
        _ = Register("base32z", 'h', static bytes => Base32z.Codec.Encode(bytes, false), static s => ToByteArray(Base32z.Codec.Decode(s)));
    }

    /// <summary>
    ///   The name of the algorithm.
    /// </summary>
    /// <value>
    ///   A unique name.
    /// </value>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///   The IPFS code assigned to the algorithm.
    /// </summary>
    /// <value>
    ///   Valid codes at <see href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/>.
    /// </value>
    public char Code { get; private set; }

    /// <summary>
    ///   Returns a function that can return a string from a byte array.
    /// </summary>
    public Func<byte[], string> Encode { get; private set; } = static _ => throw new NotImplementedException();

    /// <summary>
    ///   Returns a function that can return a byte array from a string.
    /// </summary>
    public Func<string, byte[]> Decode { get; private set; } = static _ => throw new NotImplementedException();

    /// <summary>
    ///   Use <see cref="Register"/> to create a new instance of a <see cref="MultiBaseAlgorithm"/>.
    /// </summary>
    private MultiBaseAlgorithm()
    {
    }

    /// <summary>
    ///   The <see cref="Name"/> of the algorithm.
    /// </summary>
    public override string ToString() => Name;

    /// <summary>
    ///   Register a new IPFS algorithm.
    /// </summary>
    /// <param name="name">
    ///   The name of the algorithm.
    /// </param>
    /// <param name="code">
    ///   The IPFS code assigned to thealgorithm.
    /// </param>
    /// <param name="encode">
    ///   A <c>Func</c> to encode a byte array.  If not specified, then a <c>Func</c> is created to
    ///   throw a <see cref="NotImplementedException"/>.
    /// </param>
    /// <param name="decode">
    ///   A <c>Func</c> to decode a string.  If not specified, then a <c>Func</c> is created to
    ///   throw a <see cref="NotImplementedException"/>.
    /// </param>
    /// <returns>
    ///   A new <see cref="MultiBaseAlgorithm"/>.
    /// </returns>
    public static MultiBaseAlgorithm Register(
        string name,
        char code,
        Func<byte[], string>? encode = null,
        Func<string, byte[]>? decode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (Names.ContainsKey(name))
        {
            throw new ArgumentException($"The IPFS multi-base algorithm name '{name}' is already defined.");
        }

        if (Codes.ContainsKey(code))
        {
            throw new ArgumentException($"The IPFS multi-base algorithm code '{code}' is already defined.");
        }

        encode ??= _ => throw new NotImplementedException($"The IPFS encode multi-base algorithm '{name}' is not implemented.");
        decode ??= _ => throw new NotImplementedException($"The IPFS decode multi-base algorithm '{name}' is not implemented.");

        var a = new MultiBaseAlgorithm
        {
            Name = name,
            Code = code,
            Encode = encode,
            Decode = decode
        };
        Names[name] = a;
        Codes[code] = a;
        return a;
    }

    /// <summary>
    ///   Remove an IPFS algorithm from the registry.
    /// </summary>
    /// <param name="algorithm">
    ///   The <see cref="MultiBaseAlgorithm"/> to remove.
    /// </param>
    public static void Deregister(MultiBaseAlgorithm algorithm)
    {
        _ = Names.Remove(algorithm.Name);
        _ = Codes.Remove(algorithm.Code);
    }

    /// <summary>
    ///   A sequence consisting of all algorithms.
    /// </summary>
    public static IEnumerable<MultiBaseAlgorithm> All => Names.Values;

    private static string HexLower(byte[] bytes) => Hex(bytes, false);
    private static string HexUpper(byte[] bytes) => Hex(bytes, true);
    private static string Hex(byte[] bytes, bool upper)
    {
        if (bytes is null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }
        char[] c = new char[bytes.Length * 2];
        int i = 0;
        foreach (byte b in bytes)
        {
            int hi = b >> 4;
            int lo = b & 0xF;
            c[i++] = ToHexChar(hi, upper);
            c[i++] = ToHexChar(lo, upper);
        }
        return new string(c);
    }
    private static char ToHexChar(int value, bool upper) => (char)(value < 10 ? '0' + value : (upper ? 'A' : 'a') + (value - 10));

    private static byte[] ToByteArray(ReadOnlySpan<byte> span) => span.ToArray();
}
