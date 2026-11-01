# Architectural concerns and improvement priorities

These are review recommendations based on the current source, not a claim that every item is an exploitable defect.

## Resolved in the .NET 10 upgrade work

### Defensive copies protect content-address invariants

`MultiHash.Digest` now returns a copy, `DagNode` snapshots its input bytes and links, and `DagLink` clones the CID without freezing the caller's instance (`src/MultiHash.cs`, `src/DagNode.cs`, `src/DagLink.cs`). `MultiAddress.Protocols` remains a mutable collection because changing that public API's mutation behavior needs separate compatibility review (`src/MultiAddress.cs`).

### Registry mutation and reads are synchronized

The registries now use concurrent dictionaries, synchronize registration and deregistration across paired indexes, and enumerate snapshots (`src/Registry/Codec.cs`, `src/Registry/HashingAlgorithm.cs`, `src/Registry/MultiBaseAlgorithm .cs`, `src/NetworkProtocol.cs`). Focused concurrency coverage currently exercises hash registration and lookup; extend it to deregistration and the other registries (`test/Registry/HashingAlgorithmTest.cs`).

### Serializer support is public and testable

System.Text.Json converter setup is now available through public `JsonCompat.ConfigureDefaultConverters`, alongside compatible serialize/deserialize helpers, while the Newtonsoft converters remain supported (`src/Serialization/JsonCompat.cs`). CID serialization through System.Text.Json has a direct test; consider equivalent tests for MultiHash and MultiAddress (`test/CidTest.cs`).

## Remaining concerns

### Framework-specific dependencies can drift

The project targets five frameworks and selects SimpleBase versions by target framework (`src/IpfsCore.csproj`). The .NET 10 unit suite covers canonical casing, invalid Base32 characters, and round trips; keep those contracts tested when changing dependency versions (`test/Base32Test.cs`, `test/MultBaseTest.cs`, `test/MultiHashTest.cs`).

### Documentation and CI have drifted from source

The Core API article lists the removed Object API and does not list current `IMfsApi`; the DocFX metadata uses `netcoreapp3.0` while current projects target .NET Standard/.NET 8/.NET 9/.NET 10 (`doc/articles/core-api.md`, `doc/docfx.json`, `src/IpfsCore.csproj`, `src/CoreApi/ICoreApi.cs`). CI does not build the docs (`.github/workflows/build.yml`, `.github/workflows/publish.yml`). Generate API pages from the current target framework and add a documentation build/link check to CI.

## Validation evidence

With .NET SDK 10.0.403 and the released .NET 10 analyzer baseline, the Release build succeeded for every target, all 214 tests passed on `net10.0`, and Release package generation succeeded. Earlier review-time failures around Base32 casing, `DagCid` conversion, invalid multibase strings, and onion-address casing are corrected and covered by the suite (`src/IpfsCore.csproj`, `test/IpfsCoreTests.csproj`, `test/Base32Test.cs`, `test/DagCidTest.cs`, `test/MultBaseTest.cs`, `test/MultiAddressTest.cs`, `test/MultiHashTest.cs`).

## Lower-priority scope clarity

The README presents a broad Core API surface, but the production project contains interfaces and data types rather than concrete node, storage, or transport implementations (`README.md`, `src/CoreApi/ICoreApi.cs`, `src/IpfsCore.csproj`). State this boundary prominently so consumers understand that a companion implementation is required.
