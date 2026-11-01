# Development conventions

## C# and API design

- Shared build settings enable nullable annotations, implicit usings, preview language features, the released .NET 10 analyzer baseline, code-style enforcement, and warnings-as-errors (`Directory.Build.props`).
- Production types use the `Ipfs` namespace; API contracts use `Ipfs.CoreApi` (`src/Cid.cs`, `src/CoreApi/ICoreApi.cs`).
- Core public types generally expose explicit encode/decode or read/write operations and value equality where appropriate (`src/Cid.cs`, `src/MultiHash.cs`, `src/MultiAddress.cs`).
- API operations are asynchronous and commonly accept an optional `CancellationToken` (`src/CoreApi/IBlockApi.cs`).
- Extension points are public registration methods for algorithm/codec metadata and implementations (`src/Registry/HashingAlgorithm.cs`, `src/Registry/MultiBaseAlgorithm .cs`, `src/Registry/Codec.cs`, `src/NetworkProtocol.cs`).

## Serialization and wire formats

- JSON representation for `Cid`, `MultiHash`, and `MultiAddress` is a string with Newtonsoft converters attached to the types (`src/Cid.cs`, `src/MultiHash.cs`, `src/MultiAddress.cs`).
- `JsonCompat` exposes public System.Text.Json converter configuration and compatibility serialization helpers (`src/Serialization/JsonCompat.cs`).
- Binary encodings use varints and Google.Protobuf stream APIs (`src/VarInt.cs`, `src/ProtobufHelper.cs`, `src/Cid.cs`, `src/MultiHash.cs`, `src/DagNode.cs`).

## Tests and changes

- Tests use MSTest `[TestClass]` and `[TestMethod]`; test files are grouped by the production type or feature (`test/CidTest.cs`, `test/Registry/HashingAlgorithmTest.cs`, `test/CoreApi/BitswapLedgerTest.cs`).
- Existing tests commonly exercise known encodings, invalid inputs, value equality, and round trips (`test/CidTest.cs`, `test/MultiAddressTest.cs`, `test/MultBaseTest.cs`).
- `[TODO]` No explicit API compatibility policy or serializer-support matrix was found in the repository. Public API changes should therefore be evaluated against the shipped NuGet package and its release notes (`src/IpfsCore.csproj`).
