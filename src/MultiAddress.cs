using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using Newtonsoft.Json;

namespace Ipfs;

/// <summary>
/// A set of steps describing how to build up a connection.
/// </summary>
/// <remarks>
/// A multi address emphasizes explicitness, self-description, and portability. It allows
/// applications to treat addresses as opaque tokens which avoids making assumptions about the
/// address representation (e.g. length).
/// <para>
/// A multi address is represented as a series of protocol codes and values pairs. For example,
/// an IPFS file at a specific address over ipv4 and tcp is "/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC".
/// </para>
/// <para>A multi address is considered immutable and value type equality is implemented.</para>
/// </remarks>
/// <seealso href="https://github.com/multiformats/multiaddr"/>
[JsonConverter(typeof(Json))]
public class MultiAddress : IEquatable<MultiAddress>
{
    /// <summary>
    /// Creates a new instance of the <see cref="MultiAddress"/> class.
    /// </summary>
    public MultiAddress() => Protocols = [];

    /// <summary>
    /// Creates a new instance of the <see cref="MultiAddress"/> class with the string.
    /// </summary>
    /// <param name="s">The string representation of a multi address, such as "/ip4/1270.0.01/tcp/5001".</param>
    public MultiAddress(string s) : this()
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return;
        }

        Read(new StringReader(s));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MultiAddress"/> class from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">
    /// A <see cref="Stream"/> containing the binary representation of a <b>MultiAddress</b>.
    /// </param>
    /// <remarks>
    /// Reads the binary representation of <see cref="MultiAddress"/> from the <paramref name="stream"/>.
    /// <para>
    /// The binary representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </para>
    /// </remarks>
    public MultiAddress(Stream stream)
        : this() => Read(stream);

    /// <summary>
    /// Creates a new instance of the <see cref="MultiAddress"/> class from the specified <see cref="IPAddress"/>.
    /// </summary>
    public MultiAddress(IPAddress ip)
        : this()
    {
        if (ip is null)
        {
            throw new ArgumentNullException(nameof(ip));
        }

        string type = ip.AddressFamily == AddressFamily.InterNetwork
            ? "ip4" : "ip6";
        Read(new StringReader($"/{type}/{ip}"));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MultiAddress"/> class from the specified <see cref="IPEndPoint"/>.
    /// </summary>
    public MultiAddress(IPEndPoint endpoint)
        : this()
    {
        if (endpoint is null)
        {
            throw new ArgumentNullException(nameof(endpoint));
        }

        string type = endpoint.AddressFamily == AddressFamily.InterNetwork ? "ip4" : "ip6";
        Read(new StringReader($"/{type}/{endpoint.Address}/tcp/{endpoint.Port}"));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MultiAddress"/> class from the specified byte array.
    /// </summary>
    /// <param name="buffer">( A byte array containing the binary representation of a <b>MultiAddress</b>.</param>
    /// <remarks>
    /// Reads the binary representation of <see cref="MultiAddress"/> from the <paramref name="buffer"/>.
    /// <para>
    /// The binary representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </para>
    /// </remarks>
    public MultiAddress(byte[] buffer)
        : this()
    {
        if (buffer == null || buffer.Length == 0)
        {
            return;
        }

        Read(new MemoryStream(buffer, false));
    }

    /// <summary>
    /// Determines if the peer ID is present.
    /// </summary>
    /// <value><b>true</b> if the peer ID present; otherwise, <b>false</b>.</value>
    /// <remarks>
    /// The peer ID is contained in the last protocol that is "ipfs" or "p2p". For example, <c>/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC</c>.
    /// </remarks>
    public bool HasPeerId => Protocols.Any(p => p.Name is "ipfs" or "p2p");

    /// <summary>
    /// Gets the peer ID of the multiaddress.
    /// </summary>
    /// <value>The <see cref="Peer.Id"/> as a <see cref="MultiHash"/>.</value>
    /// <exception cref="Exception">
    /// When the last <see cref="Protocols">protocol</see> is not "ipfs" nor "p2p".
    /// </exception>
    /// <remarks>
    /// The peer ID is contained in the last protocol that is "ipfs" or "p2p". For example, <c>/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC</c>.
    /// </remarks>
    public MultiHash PeerId
    {
        get
        {
            NetworkProtocol? protocol = Protocols
                .LastOrDefault(p => p.Name is "ipfs" or "p2p");
            return protocol?.Value is null
                ? throw new InvalidOperationException($"'{this}' is missing the peer ID. Add the 'ipfs' or 'p2p' protocol.")
                : (MultiHash)protocol.Value;
        }
    }

    /// <summary>
    /// The components of the <b>MultiAddress</b>.
    /// </summary>
    public System.Collections.ObjectModel.Collection<NetworkProtocol> Protocols { get; private set; }

    /// <summary>
    /// Implicit casting of a <see cref="string"/> to a <see cref="MultiAddress"/>.
    /// </summary>
    /// <param name="s">The string representation of a <see cref="MultiAddress"/>.</param>
    /// <returns>A new <see cref="MultiAddress"/>.</returns>
    public static implicit operator MultiAddress(string s) => new(s);

    /// <summary>
    /// Value inequality.
    /// </summary>
    public static bool operator !=(MultiAddress? a, MultiAddress? b) => !(a == b);

    /// <summary>
    /// Value equality.
    /// </summary>
    public static bool operator ==(MultiAddress? a, MultiAddress? b) => ReferenceEquals(a, b) || (a is not null && b is not null && a.Equals(b));

    /// <summary>
    /// Try to create a <see cref="MultiAddress"/> from the specified string.
    /// </summary>
    /// <param name="s">The string representation of a multi address, such as "/ip4/1270.0.01/tcp/5001".</param>
    /// <returns><b>null</b> if the string cannot be parsed; otherwise a <see cref="MultiAddress"/>.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public static MultiAddress? TryCreate(string s)
    {
        try
        {
            return new MultiAddress(s);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Try to create a <see cref="MultiAddress"/> from the specified the binary encoding.
    /// </summary>
    /// <param name="bytes">The binary encoding of a multiaddress.</param>
    /// <returns><b>null</b> if the bytes cannot be parsed; otherwise a <see cref="MultiAddress"/>.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public static MultiAddress? TryCreate(byte[] bytes)
    {
        try
        {
            return new MultiAddress(bytes);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a deep copy of the multi address.
    /// </summary>
    /// <returns>A new deep copy.</returns>
    public MultiAddress Clone() => new(ToString());

    /// <inheritdoc/>
    public override bool Equals(object? obj) => (obj is MultiAddress that) && Equals(that);

    /// <inheritdoc/>
    public bool Equals(MultiAddress? that)
    {
        if (Protocols.Count != that?.Protocols.Count)
        {
            return false;
        }

        for (int i = 0; i < Protocols.Count; ++i)
        {
            if (Protocols[i].Code != that.Protocols[i].Code)
            {
                return false;
            }

            if (Protocols[i].Value != that.Protocols[i].Value)
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int code = 0;
        foreach (NetworkProtocol p in Protocols)
        {
            code += p.Code.GetHashCode();
            if (p.Value is string s)
            {
                code += s.GetHashCode(System.StringComparison.Ordinal);
            }
            else
            {
#if NETSTANDARD2_1_OR_GREATER
                code += p.Value?.GetHashCode(StringComparison.Ordinal) ?? 0;
#else
                code += p.Value?.GetHashCode() ?? 0;
#endif
            }
        }
        return code;
    }

    /// <summary>
    /// Returns the IPFS binary representation as a byte array.
    /// </summary>
    /// <returns>A byte array.</returns>
    /// <remarks>
    /// The binary representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </remarks>
    public byte[] ToArray()
    {
        using var ms = new MemoryStream();
        Write(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Returns a new <see cref="MultiAddress"/> that is a copy of this instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="MultiAddress"/> that is a copy of this instance.
    /// </returns>
    public MultiAddress ToMultiAddress() => new() { Protocols = [.. Protocols] };

    /// <summary>
    /// A sequence of <see cref="NetworkProtocol">network protocols</see> that is readable to a human.
    /// </summary>
    public override string ToString()
    {
        using var s = new StringWriter();
        Write(s);
        return s.ToString();
    }

    /// <summary>
    /// Gets a multiaddress without the peer ID.
    /// </summary>
    /// <returns>
    /// Either the this multiaddress when it does not contain a peer ID; or a new <see
    /// cref="MultiAddress"/> without the peer ID.
    /// </returns>
    public MultiAddress WithoutPeerId()
    {
        if (!HasPeerId)
        {
            return this;
        }
        MultiAddress clone = Clone();
        for (int i = clone.Protocols.Count - 1; i >= 0; i--)
        {
            if (clone.Protocols[i].Name is "p2p" or "ipfs")
            {
                clone.Protocols.RemoveAt(i);
            }
        }
        return clone;
    }

    /// <summary>
    /// Gets a multiaddress that ends with the peer ID.
    /// </summary>
    /// <param name="peerId">The peer ID to end the multiaddress with.</param>
    /// <returns>
    /// Either the <c>this</c> multiadddress when it contains the <paramref name="peerId"/> or a
    /// new <see cref="MultiAddress"/> ending the <paramref name="peerId"/>.
    /// </returns>
    /// <exception cref="Exception">When the mulltiaddress has the wrong peer ID.</exception>
    public MultiAddress WithPeerId(MultiHash peerId)
    {
        if (HasPeerId)
        {
            MultiHash id = PeerId;
            return id != peerId ? throw new InvalidOperationException($"Expected a multiaddress with peer ID of '{peerId}', not '{id}'.") : this;
        }

        return new MultiAddress(ToString() + $"/p2p/{peerId}");
    }

    /// <summary>
    /// Writes the binary representation to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to write to.</param>
    /// <remarks>
    /// The binary representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </remarks>
    public void Write(Stream stream)
    {
        using var cos = new CodedOutputStream(stream, true);
        Write(cos);
        cos.Flush();
    }

    /// <summary>
    /// Writes the binary representation to the specified <see cref="CodedOutputStream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="CodedOutputStream"/> to write to.</param>
    /// <remarks>
    /// The binary representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </remarks>
    public void Write(CodedOutputStream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        foreach (NetworkProtocol protocol in Protocols)
        {
            stream.WriteInt64(protocol.Code);
            protocol.WriteValue(stream);
        }
    }

    /// <summary>
    /// Writes the string representation to the specified <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="stream">The <see cref="TextWriter"/> to write to.</param>
    /// <remarks>
    /// The string representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </remarks>
    public void Write(TextWriter stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        foreach (NetworkProtocol protocol in Protocols)
        {
            stream.Write('/');
            stream.Write(protocol.Name);
            protocol.WriteValue(stream);
        }
    }

    /// <summary>
    /// Reads the binary representation of the the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <remarks>
    /// The binary representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </remarks>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    private void Read(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        Read(new CodedInputStream(stream, true));
    }

    /// <summary>
    /// Reads the binary representation of the specified <see cref="CodedInputStream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="CodedInputStream"/> to read from.</param>
    /// <remarks>
    /// The binary representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </remarks>
    private void Read(CodedInputStream stream)
    {
        Protocols.Clear();
        do
        {
            uint code = (uint)stream.ReadInt64();
            if (!NetworkProtocol.Codes.TryGetValue(code, out Type? protocolType))
            {
                throw new InvalidDataException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "The IPFS network protocol code '{0}' is unknown.", code));
            }

            NetworkProtocol p = (NetworkProtocol?)Activator.CreateInstance(protocolType)
                ?? throw new InvalidDataException($"Cannot create an instance of '{protocolType}'.");
            p.ReadValue(stream);
            Protocols.Add(p);
        } while (!stream.IsAtEnd);
    }

    /// <summary>
    /// Reads the string representation from the specified <see cref="TextReader"/>.
    /// </summary>
    /// <param name="stream">The <see cref="TextReader"/> to read from</param>
    /// <remarks>
    /// The string representation is a sequence of <see cref="NetworkProtocol">network protocols</see>.
    /// </remarks>
    private void Read(TextReader stream)
    {
        if (stream.Read() != '/')
        {
            throw new FormatException("An IPFS multiaddr must start with '/'.");
        }

        var name = new StringBuilder();
        Protocols.Clear();
        while (true)
        {
            _ = name.Clear();

            int c;
            while (-1 != (c = stream.Read()) && c != '/')
            {
                _ = name.Append((char)c);
            }

            if (name.Length == 0)
            {
                break;
            }

            if (!NetworkProtocol.Names.TryGetValue(name.ToString(), out Type? protocolType))
            {
                throw new FormatException($"The IPFS network protocol '{name}' is unknown.");
            }

            NetworkProtocol p = (NetworkProtocol?)Activator.CreateInstance(protocolType)
                ?? throw new InvalidDataException($"Cannot create an instance of '{protocolType}'.");
            p.ReadValue(stream);
            Protocols.Add(p);
        }

        if (Protocols.Count == 0)
        {
            throw new FormatException("The IPFS multiaddr has no protocol specified.");
        }
    }

    /// <summary>
    /// Conversion of a <see cref="MultiAddress"/> to and from JSON.
    /// </summary>
    /// <remarks>The JSON is just a single string value.</remarks>
    private sealed class Json : JsonConverter
    {
        /// <summary>
        /// A singleton instance of the <see cref="Json"/> converter.
        /// </summary>
        public static readonly Json Instance = new();

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) => true;

        /// <inheritdoc/>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => reader.Value is string s ? new MultiAddress(s) : null;

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var ma = value as MultiAddress;
            writer.WriteValue(ma?.ToString());
        }
    }
}
