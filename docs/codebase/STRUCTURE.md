# Repository structure

```text
src/
  CoreApi/       IPFS API interfaces and their data/option types
  Cryptography/  HashAlgorithm adapters and specialized digest implementations
  Registry/      Codec, multihash, and multibase registries
  Serialization/ JSON compatibility helpers
  *.cs           CIDs, multihashes, multiaddresses, DAG types, peers, and encoding utilities
test/
  CoreApi/       Tests for Core API data types
  Cryptography/  Hash implementation tests
  Registry/      Registry behavior tests
  *.cs           Tests for core value types and binary/text encodings
doc/
  articles/      DocFX articles for core concepts and API contracts
  docfx.json     DocFX metadata and site configuration
```

The solution has one production project and one test project (`IpfsCore.sln`). The production project is a single NuGet library rather than a host or network service (`src/IpfsCore.csproj`).

## Entry points and boundaries

- `src/CoreApi/ICoreApi.cs` aggregates the public API contracts. Its members are feature interfaces; this repository does not contain concrete Kubo/HTTP or node-service implementations.
- `src/Cid.cs`, `src/MultiHash.cs`, `src/MultiAddress.cs`, and `src/DagNode.cs` hold the main content-addressing, addressing, and DAG models.
- `src/Registry/` contains registries referenced by the encoders and decoders in the root `src/` types.
- `test/IpfsCoreTests.csproj` references the library project directly. Tests are organized by type/feature rather than by a separate integration-test project (`test/`).

## Missing or uncertain repository artifacts

- `[TODO]` `TestManual.md` and `AGENTS.md` were not present at the repository root during this review.
- `[TODO]` No sample server/client projects were found; the runnable validation path is the test project and GitHub Actions workflows (`IpfsCore.sln`, `test/IpfsCoreTests.csproj`, `.github/workflows/build.yml`).
