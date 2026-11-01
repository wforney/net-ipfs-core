# Technology stack

## Runtime and projects

- The solution contains the `IpfsCore` library and `IpfsCoreTests` test project (`IpfsCore.sln`).
- The library targets `netstandard2.0`, `netstandard2.1`, `net8.0`, `net9.0`, and `net10.0`; tests target `net10.0` (`src/IpfsCore.csproj`, `test/IpfsCoreTests.csproj`).
- Shared build settings enable nullable reference types, preview language features, the released .NET 10 analyzer baseline, and warnings-as-errors (`Directory.Build.props`).

## Key dependencies

- Google.Protobuf provides wire-format primitives used by CID, MultiHash, MultiAddress, and DAG serialization (`src/IpfsCore.csproj`, `src/Cid.cs`, `src/MultiHash.cs`, `src/MultiAddress.cs`, `src/DagNode.cs`).
- Portable.BouncyCastle supplies digest implementations beyond the platform hash algorithms (`src/IpfsCore.csproj`, `src/Registry/HashingAlgorithm.cs`).
- Newtonsoft.Json is part of the existing converter surface; System.Text.Json is also referenced and has public compatibility helpers and converters (`src/IpfsCore.csproj`, `src/Cid.cs`, `src/MultiHash.cs`, `src/MultiAddress.cs`, `src/Serialization/JsonCompat.cs`).
- SimpleBase is versioned per target framework, ranging from 2.1.0 to 5.5.0 (`src/IpfsCore.csproj`).
- MSTest and Microsoft.NET.Test.Sdk run the test suite (`test/IpfsCoreTests.csproj`).

## Build and delivery

- The build-validation workflow uses .NET 10 and runs `dotnet build /r` and `dotnet test` (`.github/workflows/build.yml`).
- The NuGet publishing workflow restores, builds Release, tests, and pushes the package (`.github/workflows/publish.yml`).
- The README documents DocFX generation, but the listed GitHub workflows do not build documentation (`README.md`, `doc/docfx.json`, `.github/workflows/build.yml`, `.github/workflows/publish.yml`).
