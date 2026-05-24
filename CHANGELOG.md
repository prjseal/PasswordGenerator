# Changelog

All notable changes to this project are documented here. This project adheres to
[Semantic Versioning](https://semver.org/).

## 3.0.0

A major release focused on cryptographic correctness, a modern API, and broader use cases.
See the [v2 → v3 migration guide](docs/v3-target/migration-v2-to-v3.md).

### Breaking changes
- **Invalid settings now throw** `ArgumentException` from `Next()` instead of returning an error
  message as the "password". Use `TryNext(out var password)` for a non-throwing path.

### Security / correctness fixes
- Cryptographically secure RNG (`CryptoRandomSource`) with **unbiased** integer sampling
  (rejection sampling — removes modulo bias).
- Fixed an off-by-one in length handling and removed the GUID-based shuffle in favour of a
  Fisher–Yates shuffle.
- Disposed/owned RNG lifecycle; removed dead code and the static RNG.
- Empty special-character sets are validated rather than silently producing weaker output.

### Added
- **Async APIs:** `NextAsync`, `GenerateAsync`.
- **Dependency injection:** `AddPasswordGenerator(...)` with code and `appSettings.json` binding
  (resolution order: code-configure > appSettings > default).
- **Presets:** `ForOwasp`, `ForNist`, `ForOtp`, `ForApiKey`, `ForEnvironmentName`, `ForPassphrase`.
- **Custom pools:** `WithCharacters(string)`, `WithAllAscii()`.
- **Quality controls:** `ExcludeAmbiguous()`, `RequireAtLeast(CharacterClass, count)`.
- **Entropy estimation:** `IEntropyEstimator` / `PoolEntropyEstimator` and `EstimateEntropyBits()`.
- **Batch API:** `Generate(count)` and a parameterless `Generate()` driven by
  `PasswordOptions.DefaultBatchCount`.

### Packaging
- Multi-targets `netstandard2.0` and `net8.0`; nullable reference types enabled.
- Single source of version truth in the csproj (removed the stale `.nuspec`).
- `PackageIcon` + `PackageReadmeFile` (clears `NU5048`), SourceLink, deterministic build, and a
  `.snupkg` symbol package.

### Compatibility
- The v2 surface (`Next`, `NextGroup`, constructors, `IncludeX`, `LengthRequired`) is unchanged and
  continues to work, aside from the error-handling breaking change noted above.

## 2.1.0 and earlier

See the project history and the original review in
[`docs/V3_REVIEW_AND_DOCUMENTATION.md`](docs/V3_REVIEW_AND_DOCUMENTATION.md).
