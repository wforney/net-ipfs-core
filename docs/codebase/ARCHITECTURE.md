# Architecture

## Scope and layers

IPFS Core is a foundational library: it supplies wire-format/value types and interfaces for higher-level IPFS clients, rather than implementing a daemon, transport, or storage service (`README.md`, `src/IpfsCore.csproj`, `src/CoreApi/ICoreApi.cs`).

The main dependency direction is:

1. The `Ipfs.CoreApi` interfaces express operations and use domain types such as `Cid`, `MultiHash`, `MultiAddress`, and `DagNode` (`src/CoreApi/ICoreApi.cs`, `src/CoreApi/IBlockApi.cs`).
2. Core value types encode/decode their own protocol representations and consult shared registries (`src/Cid.cs`, `src/MultiHash.cs`, `src/MultiAddress.cs`, `src/MultiBase.cs`, `src/Registry/Codec.cs`, `src/Registry/HashingAlgorithm.cs`, `src/Registry/MultiBaseAlgorithm .cs`).
3. Binary formats use varints and Google.Protobuf stream primitives; the DAG node serializes links and data and derives its ID from serialized bytes (`src/VarInt.cs`, `src/ProtobufHelper.cs`, `src/Cid.cs`, `src/MultiHash.cs`, `src/DagNode.cs`).

## Core models and registries

- `Cid` represents CIDv0/v1 and encodes a multihash with a codec and multibase for v1; non-default v0 properties can promote the value to v1 (`src/Cid.cs`).
- `MultiHash` records an algorithm code and digest, computes hashes using registered implementations, and can parse unknown algorithm codes as metadata-only entries (`src/MultiHash.cs`, `src/Registry/HashingAlgorithm.cs`).
- `MultiAddress` composes registered protocol steps and serializes the same component model as text or binary (`src/MultiAddress.cs`, `src/NetworkProtocol.cs`).
- `DagNode` serializes links and opaque data and computes a CID from those bytes; its constructor orders links by name (`src/DagNode.cs`, `src/DagLink.cs`).
- Registries provide extensibility for multicodecs, hash algorithms, multibase encodings, and network protocols (`src/Registry/Codec.cs`, `src/Registry/HashingAlgorithm.cs`, `src/Registry/MultiBaseAlgorithm .cs`, `src/NetworkProtocol.cs`).

## Architectural strengths

- Protocol concerns are represented by small domain types and feature-specific API interfaces rather than being coupled to one transport (`src/CoreApi/ICoreApi.cs`, `src/CoreApi/IBlockApi.cs`).
- Core encodings are covered by known-vector and round-trip tests (`test/CidTest.cs`, `test/MultiHashTest.cs`, `test/MultiAddressTest.cs`, `test/DagNodeTest.cs`, `test/MultBaseTest.cs`).
- Multiple target frameworks allow the same library API to serve older .NET Standard consumers and current .NET applications (`src/IpfsCore.csproj`).

## Recommended architectural improvements

1. **Finish enforcing value invariants.** `MultiHash.Digest` now returns a defensive copy, `DagNode` copies its input bytes and links, and `DagLink` clones its CID without freezing the caller's CID (`src/MultiHash.cs`, `src/DagNode.cs`, `src/DagLink.cs`). `MultiAddress.Protocols` remains a mutable public collection for compatibility, so its contents can still be changed by callers (`src/MultiAddress.cs`).
2. **Continue testing registry concurrency across all registry types.** Registry storage now uses `ConcurrentDictionary`, registration updates paired indexes under locks, and enumeration returns snapshots (`src/Registry/Codec.cs`, `src/Registry/HashingAlgorithm.cs`, `src/Registry/MultiBaseAlgorithm .cs`, `src/NetworkProtocol.cs`). The current added concurrency test exercises hash registration and lookup; deregistration and the other registries still merit focused stress coverage (`test/Registry/HashingAlgorithmTest.cs`).
3. **Preserve serializer contract coverage.** `JsonCompat` is public and exposes converter configuration for System.Text.Json, while Newtonsoft converters remain available on core types (`src/Serialization/JsonCompat.cs`, `src/Cid.cs`, `src/MultiHash.cs`, `src/MultiAddress.cs`). Consumer-style CID serialization is now directly tested (`test/CidTest.cs`); add equivalent tests for the other value types when their public converter behavior changes.
4. **Keep the API articles synchronized with the current contracts.** The Core API article still documents `IObjectApi` and omits current interfaces such as `IMfsApi`, while the source contract includes modern additions and API-specific option types (`doc/articles/core-api.md`, `src/CoreApi/ICoreApi.cs`). Add the documentation build to CI and validate links against the actual API surface (`doc/docfx.json`, `.github/workflows/build.yml`).
