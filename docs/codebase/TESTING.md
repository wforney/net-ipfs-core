# Testing

## Available validation

- The automated suite is `test/IpfsCoreTests.csproj`, targeting `net10.0` and referencing `src/IpfsCore.csproj` (`test/IpfsCoreTests.csproj`).
- The repository CI build-validation workflow runs a build and `dotnet test` with .NET 10 (`.github/workflows/build.yml`).
- The package publishing workflow restores, builds Release, and runs tests before publishing (`.github/workflows/publish.yml`).
- Tests are unit-level checks for value types, serialization, registries, hashing, and encodings. No sample app or separate integration-test project was found (`test/`, `IpfsCore.sln`).
- The README mentions DocFX generation, but the workflows do not run it (`README.md`, `doc/docfx.json`, `.github/workflows/build.yml`, `.github/workflows/publish.yml`).

## Commands

```powershell
dotnet build
dotnet test
```

`dotnet test` restores packages by default. The `--no-restore` option requires existing assets in `src/obj` and `test/obj`.

## Upgrade validation

With .NET SDK 10.0.403 and the released .NET 10 analyzer baseline, the Release library build succeeded across all five target frameworks and all 214 tests passed on `net10.0`. Release package generation also succeeded. Nullable checks, code-style enforcement, and warnings-as-errors remain enabled.

## Gaps to consider

- Add System.Text.Json consumer tests for MultiHash and MultiAddress, complementing the CID test (`src/Serialization/JsonCompat.cs`, `test/CidTest.cs`, `test/MultiHashTest.cs`, `test/MultiAddressTest.cs`).
- Expand registry concurrency tests to cover deregistration and all registry implementations (`src/Registry/HashingAlgorithm.cs`, `src/Registry/MultiBaseAlgorithm .cs`, `src/Registry/Codec.cs`).
- Add integration coverage in a separate project if this repository later takes responsibility for concrete API implementations; today its API types are contracts (`src/CoreApi/ICoreApi.cs`).
