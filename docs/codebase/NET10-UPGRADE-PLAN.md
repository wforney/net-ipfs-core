# .NET 10 upgrade plan

## Goal and compatibility decision

Add .NET 10 support without removing existing target frameworks. Keep `netstandard2.0`, `netstandard2.1`, `net8.0`, and `net9.0` for current consumers, add `net10.0` to the library, and move the test project and CI SDK to .NET 10 (`src/IpfsCore.csproj`, `test/IpfsCoreTests.csproj`, `.github/workflows/build.yml`, `.github/workflows/publish.yml`).

## Work sequence

1. **Framework and pipeline**: add `net10.0` to the library target list; retarget tests to `net10.0`; run builds for every library TFM; update CI build and package-publish workflows to install .NET 10.
2. **Dependency compatibility**: verify each direct dependency restores and builds across all supported TFMs. Upgrade the System.Text.Json package to the .NET 10 line. Preserve the Portable.BouncyCastle 1.8.5 pin until SHAKE-128 output sizing is fixed and covered; its project comment records the 1.9.0 regression (`src/IpfsCore.csproj`).
3. **Correctness fixes**: enforce lowercase RFC 4648 Base32 output; make onion multiaddress output canonical lowercase; reject invalid base64url characters; make explicit conversion of a `libp2p-key` CID to `DagCid` throw the documented exception.
4. **Value safety and registry reliability**: defensively copy `MultiHash` digests and DAG node input data/links; make registry storage safe for concurrent lookup and mutation, with atomic name/code updates and a regression test for alias deregistration.
5. **Serialization and docs**: expose an additive public System.Text.Json converter-registration API while keeping Newtonsoft behavior; test both serializers. Bring the Core API article into agreement with current interfaces and add a DocFX build to CI only if the repository has a supported, reproducible DocFX install path.
6. **Validation and release readiness**: run Release builds for every target, the full .NET 10 test suite, dependency vulnerability checks, and package creation; update the README supported-framework list and release notes.

The shared build uses the released .NET 10 analyzer baseline instead of `preview-all`; nullable analysis, code-style enforcement, and warnings-as-errors remain enabled.

## API compatibility guardrails

- Keep all current target frameworks and existing public signatures.
- New API surface should be additive; do not remove or change interface members.
- Returning a defensive copy from `MultiHash.Digest` preserves its signature while preventing external mutation of the stored digest.
- `MultiAddress.Protocols` remains a mutable `Collection<NetworkProtocol>` for compatibility. Fully enforcing immutability there requires a separately reviewed API migration; do not silently change its mutation semantics during this upgrade.
- `[TODO]` Decide whether to standardize all consumers on .NET 10 in a later major release. This change only adds .NET 10 support.

## Acceptance criteria

- `dotnet build` succeeds for every library target with the repository's warnings-as-errors configuration.
- `dotnet test` passes on .NET 10, including Base32 vectors, CID/DagCid validation, multibase invalid-input behavior, onion canonicalization, and registry concurrency/alias lifecycle.
- CI validates .NET 10 build and tests; package publishing uses the same SDK.
- Existing .NET Standard, .NET 8, and .NET 9 consumers retain their package assets and public API.

## Validation status

Using .NET SDK 10.0.403 and the released .NET 10 analyzer baseline, the Release library build succeeded across all five target frameworks, all 214 tests passed on `net10.0`, and Release package generation succeeded. The package audit reported no vulnerable packages from configured sources.
