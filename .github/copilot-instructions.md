# IPFS Core .NET - AI Coding Instructions

## Project Overview

This is the core library for IPFS (InterPlanetary File System) implementation in .NET, providing fundamental objects and interfaces for content-addressed, distributed file systems. The library targets .NET Standard 2.0+ and serves as the foundation for IPFS implementations in multiple .NET languages.

## Architecture & Core Components

### Major Objects Hierarchy
- **Cid (Content Identifier)**: Self-describing content addresses with version (0/1), encoding (base58btc/base32), content type (dag-pb), and MultiHash
- **MultiHash**: Self-describing hash format with algorithm registry supporting SHA, BLAKE2, Keccak, SHA3, SHAKE variants
- **MultiAddress**: Protocol-aware network addresses like `/ip4/10.1.10.10/tcp/80/ipfs/QmVcSq...`
- **DagNode/DagLink**: Merkle DAG structures for content-addressed data with cryptographic integrity

### API Structure Pattern
All IPFS functionality is exposed through `ICoreApi` which aggregates domain-specific APIs:
```csharp
ICoreApi.Block     // Block storage operations
ICoreApi.Dag       // Directed Acyclic Graph operations  
ICoreApi.FileSystem // File operations
ICoreApi.Pin       // Content pinning
ICoreApi.Swarm     // Peer networking
// ... 13 total sub-APIs
```

### Registry Pattern
Core types use static registries for extensibility:
- `HashingAlgorithm.Register()` for new hash algorithms with code/name mapping
- `MultiBaseAlgorithm.Register()` for encoding schemes
- `Codec.Register()` for content types
- All registries are thread-safe dictionaries with numeric codes and string names

## Development Patterns & Conventions

### Immutability & Value Semantics
- Core objects (Cid, MultiHash, MultiAddress) are immutable after encoding
- Implement `IEquatable<T>` with value equality semantics
- Use `EnsureMutable()` pattern to prevent modification after first use
- Properties throw `NotSupportedException` when immutable

### Encoding & Serialization
- **Dual JSON Support**: Gradual migration from Newtonsoft.Json to System.Text.Json
- Use `JsonCompat` utility for transparent fallback between serializers
- All core types have custom JSON converters for both systems
- Binary serialization uses protobuf for wire formats

### Version Compatibility
- CID v0: SHA2-256 + dag-pb + base58btc encoding (legacy IPFS)
- CID v1: Any hash + any codec + any encoding (modern IPFS)
- Auto-upgrade v0→v1 when non-default properties are set

### Error Handling
- Use specific exceptions: `ArgumentException` for invalid algorithms/formats
- `NotSupportedException` for immutability violations
- Validate hash digest sizes against algorithm requirements

## Build & Test Configuration

### Multi-Targeting Strategy
- Main project: `netstandard2.0;netstandard2.1;net8.0;net9.0` 
- Test project: `net9.0` only (latest features)
- Use conditional `PackageReference` for framework-specific dependencies (SimpleBase versions)

### CI/CD Pipeline
- **Build**: `dotnet build /r` (restore packages)
- **Test**: `dotnet test` (MSTest framework)
- **Documentation**: DocFX generates API docs from XML comments
- **Package**: Auto-generated on Release builds to NuGet

### Code Quality
- `TreatWarningsAsErrors=true` across all projects
- .NET analyzers enabled with preview features
- Nullable reference types enabled
- LangVersion preview for latest C# features

## Key Dependencies & Integration

### Cryptography
- **BouncyCastle**: BLAKE2, SHA3, SHAKE, Keccak implementations
- **System.Security.Cryptography**: Standard SHA1/SHA2 algorithms  
- Custom `DoubleSha256` for Bitcoin-style double hashing

### Protocol Buffers
- Google.Protobuf for binary serialization
- Embedded `.proto` files as resources
- `ProtobufHelper` utility for common operations

### Testing Conventions
- Test files follow `{Type}Test.cs` pattern
- Use `ExceptionAssert.Throws<T>()` for exception testing
- Test both string and binary representations
- Validate round-trip encoding/decoding

## Common Implementation Patterns

### Registry-Based Extensibility
```csharp
// Register new hash algorithm
HashingAlgorithm.Register("custom-hash", 0x999, 32, () => new CustomHasher());

// Register new multibase encoding  
MultiBaseAlgorithm.Register("custom-base", 'x', encoder, decoder);
```

### Stream Extension Methods
- `Stream.ReadVarint32/64()` and `Stream.WriteVarint()` for Protocol Buffer varints
- Network byte order (big-endian) for all binary formats

### Type Conversion Patterns
- Implicit conversions from strings: `Cid cid = "QmHash..."`
- ToString() overloads: `cid.ToString("L")` for long format
- `MultiBase.Encode()/Decode()` for encoding conversions

When implementing new features, follow these patterns for consistency with the existing codebase architecture.
