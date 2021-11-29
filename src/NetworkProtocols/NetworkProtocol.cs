using Google.Protobuf;

namespace Ipfs
{
    /// <summary>
    /// Metadata on an IPFS network address protocol.
    /// </summary>
    /// <remarks>
    /// Protocols are defined at <see
    /// href="https://github.com/multiformats/multiaddr/blob/master/protocols.csv" />.
    /// </remarks>
    /// <seealso cref="MultiAddress" />
    public abstract class NetworkProtocol
    {
        internal static Dictionary<uint, Type> Codes = new Dictionary<uint, Type>();
        internal static Dictionary<string, Type> Names = new Dictionary<string, Type>();

        /// <summary>
        /// Registers the standard network protocols for IPFS.
        /// </summary>
        static NetworkProtocol()
        {
            NetworkProtocol.Register<Ipv4NetworkProtocol>();
            NetworkProtocol.Register<Ipv6NetworkProtocol>();
            NetworkProtocol.Register<TcpNetworkProtocol>();
            NetworkProtocol.Register<UdpNetworkProtocol>();
            NetworkProtocol.Register<P2pNetworkProtocol>();
            NetworkProtocol.RegisterAlias<IpfsNetworkProtocol>();
            NetworkProtocol.Register<QuicNetworkProtocol>();
            NetworkProtocol.Register<HttpNetworkProtocol>();
            NetworkProtocol.Register<HttpsNetworkProtocol>();
            NetworkProtocol.Register<DccpNetworkProtocol>();
            NetworkProtocol.Register<SctpNetworkProtocol>();
            NetworkProtocol.Register<WsNetworkProtocol>();
            NetworkProtocol.Register<Libp2pWebrtcStarNetworkProtocol>();
            NetworkProtocol.Register<UdtNetworkProtocol>();
            NetworkProtocol.Register<UtpNetworkProtocol>();
            NetworkProtocol.Register<OnionNetworkProtocol>();
            NetworkProtocol.Register<Libp2pWebrtcDirectNetworkProtocol>();
            NetworkProtocol.Register<P2pCircuitNetworkProtocol>();
            NetworkProtocol.Register<DnsNetworkProtocol>();
            NetworkProtocol.Register<Dns4NetworkProtocol>();
            NetworkProtocol.Register<Dns6NetworkProtocol>();
            NetworkProtocol.Register<DnsAddrNetworkProtocol>();
            NetworkProtocol.Register<WssNetworkProtocol>();
            NetworkProtocol.Register<IpcidrNetworkProtocol>();
        }

        /// <summary>
        /// The IPFS numeric code assigned to the network protocol.
        /// </summary>
        public abstract uint Code { get; }

        /// <summary>
        /// The name of the protocol.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The string value associated with the protocol.
        /// </summary>
        /// <remarks>
        /// For tcp and udp this is the port number. This can be <b>null</b> as is the case for http
        /// and https.
        /// </remarks>
        public string? Value { get; set; }

        /// <summary>
        /// Register a network protocol for use.
        /// </summary>
        /// <typeparam name="T">A <see cref="NetworkProtocol" /> to register.</typeparam>
        public static void Register<T>() where T : NetworkProtocol, new()
        {
            var protocol = new T();

            if (Names.ContainsKey(protocol.Name))
            {
                throw new ArgumentException(string.Format("The IPFS network protocol '{0}' is already defined.", protocol.Name));
            }

            if (Codes.ContainsKey(protocol.Code))
            {
                throw new ArgumentException(string.Format("The IPFS network protocol code ({0}) is already defined.", protocol.Code));
            }

            Names.Add(protocol.Name, typeof(T));
            Codes.Add(protocol.Code, typeof(T));
        }

        /// <summary>
        /// Register an alias to another network protocol.
        /// </summary>
        /// <typeparam name="T">A <see cref="NetworkProtocol" /> to register.</typeparam>
        public static void RegisterAlias<T>() where T : NetworkProtocol, new()
        {
            var protocol = new T();

            if (Names.ContainsKey(protocol.Name))
            {
                throw new ArgumentException(string.Format("The IPFS network protocol '{0}' is already defined.", protocol.Name));
            }

            if (!Codes.ContainsKey(protocol.Code))
            {
                throw new ArgumentException(string.Format("The IPFS network protocol code ({0}) is not defined.", protocol.Code));
            }

            Names.Add(protocol.Name, typeof(T));
        }

        /// <summary>
        /// Reads the binary representation from the specified <see cref="CodedInputStream" />.
        /// </summary>
        /// <param name="stream">The <see cref="CodedOutputStream" /> to read from.</param>
        /// <remarks>The binary representation is an option <see cref="Value" />.</remarks>
        public abstract void ReadValue(CodedInputStream stream);

        /// <summary>
        /// Reads the string representation from the specified <see cref="TextReader" />.
        /// </summary>
        /// <param name="stream">The <see cref="TextReader" /> to read from</param>
        /// <remarks>
        /// The string representation is "/ <see cref="Name" />" followed by an optional "/ <see
        /// cref="Value" />".
        /// </remarks>
        public virtual void ReadValue(TextReader stream)
        {
            Value = string.Empty;
            int c;
            while (-1 != (c = stream.Read()) && c != '/')
            {
                Value += (char)c;
            }
        }

        /// <summary>
        /// The <see cref="Name" /> and optional <see cref="Value" /> of the network protocol.
        /// </summary>
        public override string ToString()
        {
            using var s = new StringWriter();
            s.Write('/');
            s.Write(Name);
            WriteValue(s);
            return s.ToString();
        }

        /// <summary>
        /// Writes the binary representation to the specified <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">The <see cref="CodedOutputStream" /> to write to.</param>
        /// <remarks>The binary representation of the <see cref="Value" />.</remarks>
        public abstract void WriteValue(CodedOutputStream stream);

        /// <summary>
        /// Writes the string representation to the specified <see cref="TextWriter" />.
        /// </summary>
        /// <param name="stream">The <see cref="TextWriter" /> to write to.</param>
        /// <remarks>The string representation of the optional <see cref="Value" />.</remarks>
        public virtual void WriteValue(TextWriter stream)
        {
            if (Value is not null)
            {
                stream.Write('/');
                stream.Write(Value);
            }
        }
    }
}
