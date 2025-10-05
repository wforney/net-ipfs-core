# 🌐 IPFS Core .NET

[![NuGet Version](https://img.shields.io/nuget/v/IpfsShipyard.Ipfs.Core.svg?style=flat-square)](https://www.nuget.org/packages/IpfsShipyard.Ipfs.Core)
[![Build Status](https://img.shields.io/github/actions/workflow/status/ipfs-shipyard/net-ipfs-core/build.yml?branch=main&style=flat-square)](https://github.com/ipfs-shipyard/net-ipfs-core/actions)
[![License](https://img.shields.io/github/license/ipfs-shipyard/net-ipfs-core.svg?style=flat-square)](LICENSE)

**The foundational .NET library for IPFS (InterPlanetary File System)** - providing content-addressed, cryptographically secure building blocks for distributed applications.

> 🚀 **The permanent web starts here.** IPFS enables creation of completely distributed applications using content addressing and cryptographic hashing. This library provides the core primitives you need to build on the decentralized web.

## ✨ Features

- 🔗 **Content Identifiers (CID)** - Self-describing content addresses with version compatibility
- 🔐 **MultiHash** - Cryptographic hash agility with 15+ algorithms (SHA-256, BLAKE2, SHA-3, etc.)  
- 🌍 **MultiAddress** - Protocol-aware network addressing for peer discovery
- 📊 **Merkle DAG** - Immutable, verifiable data structures for content integrity
- 🎯 **Multi-targeting** - .NET Standard 2.0+, .NET 8.0+, .NET 9.0 support
- ⚡ **High Performance** - Zero-allocation paths and efficient serialization

## 🚀 Quick Start

### Installation

```bash
dotnet add package IpfsShipyard.Ipfs.Core
```

### Basic Usage

```csharp
using Ipfs;

// Create content identifiers
var cid = new Cid("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V");
Console.WriteLine($"CID: {cid}");
Console.WriteLine($"Details: {cid.ToString("L")}"); // base58btc cidv0 dag-pb sha2-256

// Work with cryptographic hashes  
var hash = new MultiHash("sha2-256", Encoding.UTF8.GetBytes("hello world"));
Console.WriteLine($"Hash: {hash}");

// Network addressing
var address = new MultiAddress("/ip4/127.0.0.1/tcp/4001/p2p/QmNodeId");
Console.WriteLine($"Address: {address}");

// String conversion (implicit)
Cid cidFromString = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V";
MultiAddress addrFromString = "/dns4/node.example.com/tcp/443/wss";
```

## 🏗️ Core Architecture

### ICoreApi Structure
The library exposes IPFS functionality through a unified API surface:

```csharp
ICoreApi.Block       // Block storage operations
ICoreApi.Dag         // Directed Acyclic Graph operations  
ICoreApi.FileSystem  // File operations
ICoreApi.Pin         // Content pinning
ICoreApi.Swarm       // Peer networking
// ... 13+ specialized APIs
```

### Registry Pattern
Extend functionality through static registries:

```csharp
// Register custom hash algorithm
HashingAlgorithm.Register("custom-hash", 0x999, 32, () => new CustomHasher());

// Register custom encoding
MultiBaseAlgorithm.Register("custom-base", 'x', encoder, decoder);
```

## 📚 Core Concepts

### Content Identifiers (CID)
Self-describing content addresses that provide cryptographic guarantees:

```csharp
// CID v0 (legacy, SHA-256 + dag-pb + base58btc)
var cidV0 = new Cid("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V");

// CID v1 (modern, any hash + any codec + any encoding)  
var cidV1 = new Cid {
    Version = 1,
    ContentType = "dag-cbor",
    Hash = new MultiHash("sha2-512", data),
    Encoding = "base32"
};

// Auto-upgrade from v0 to v1 when setting non-default properties
var upgraded = new Cid("QmHash...") { Encoding = "base32" }; // Now v1
```

### MultiHash 
Cryptographic hash agility for future-proof applications:

```csharp
// Supported algorithms: SHA-1/2/3, BLAKE2b/2s, Keccak, SHAKE, and more
var sha256 = new MultiHash("sha2-256", data);
var blake2b = new MultiHash("blake2b-256", data);
var sha3 = new MultiHash("sha3-512", data);

// Validate hash digest sizes automatically
var hash = MultiHash.ComputeHash("sha2-256", Encoding.UTF8.GetBytes("hello"));
```

### MultiAddress
Protocol-aware network addressing for multi-transport connectivity:

```csharp
// IPv4 + TCP
var tcpAddr = new MultiAddress("/ip4/127.0.0.1/tcp/4001");

// DNS + WebSocket Secure  
var wsAddr = new MultiAddress("/dns4/node.example.com/tcp/443/wss");

// Complex multi-hop addressing
var complexAddr = new MultiAddress("/ip4/10.1.10.10/tcp/29087/ipfs/QmNodeId/p2p-circuit/ip4/192.168.1.1/tcp/4002");
```

### Merkle DAG
Immutable, content-addressed data structures:

```csharp
// Create DAG nodes with cryptographic links
var node = new DagNode {
    Data = Encoding.UTF8.GetBytes("Hello IPFS"),
    Links = {
        new DagLink("child1", childCid1, childSize1),
        new DagLink("child2", childCid2, childSize2)
    }
};

// Nodes are immutable after creation - no cycles possible
var nodeCid = node.Id; // Content ID derived from node content
```

## 🔧 Advanced Features

### JSON Serialization
Seamless dual JSON support with automatic fallback:

```csharp
// Works with both Newtonsoft.Json and System.Text.Json
var cid = new Cid("QmHash...");
var json = JsonConvert.SerializeObject(cid);              // Newtonsoft
var systemJson = JsonSerializer.Serialize(cid);           // System.Text.Json

// Custom JsonCompat utility handles compatibility automatically
var result = JsonCompat.Serialize(cid, forceNewtonsoft: false);
```

### Stream Extensions
Efficient Protocol Buffer varint encoding:

```csharp
using var stream = new MemoryStream();

// Write variable-length integers
stream.WriteVarint(12345L);
stream.WriteVarint(67890L);

stream.Position = 0;

// Read them back
var value1 = stream.ReadVarint64();
var value2 = stream.ReadVarint64();
```

### Immutability Patterns
Fail-fast design prevents modification after encoding:

```csharp
var cid = new Cid { Hash = hash }; // Mutable during construction
var encoded = cid.Encode();        // Now immutable

// This throws NotSupportedException
cid.Version = 1; // ❌ Cannot modify after encoding
```

## 🚢 Related Projects

- **[IPFS HTTP Client](https://github.com/ipfs-shipyard/net-ipfs-http-client)** - Full-featured .NET client for IPFS HTTP API
- **[IPFS Engine](https://github.com/ipfs-shipyard/net-ipfs-engine)** - Embedded IPFS node implementation  
- **[IPFS.NET](https://github.com/ipfs-shipyard/net-ipfs)** - Complete IPFS toolkit for .NET developers

## 🛠️ Development

### Building
```bash
git clone https://github.com/ipfs-shipyard/net-ipfs-core.git
cd net-ipfs-core

# Restore packages and build
dotnet build /r

# Run tests
dotnet test

# Generate documentation
cd doc && docfx
```

### Contributing
We welcome contributions! This project is maintained by the IPFS Shipyard community, building on the foundation established by Richard Schneider.

1. 🍴 Fork the repository
2. 🌟 Create a feature branch
3. ✅ Ensure tests pass
4. 📝 Update documentation
5. 🚀 Submit a pull request

## 📖 Documentation

- **[API Reference](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.html)** - Complete API documentation
- **[IPFS Specs](https://github.com/ipfs/specs)** - IPFS protocol specifications
- **[MultiFormats](https://github.com/multiformats)** - Self-describing format standards

## 📄 License

Licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

---

**Built with ❤️ by the IPFS Shipyard community** | [🌟 Star us on GitHub](https://github.com/ipfs-shipyard/net-ipfs-core)

