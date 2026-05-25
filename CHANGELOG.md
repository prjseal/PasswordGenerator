# Changelog

All notable changes to this project are documented here. This project adheres to
[Semantic Versioning](https://semver.org/).

## 3.0.0

A major release focused on cryptographic correctness, a modern API, and broader use cases.
See the [v2 → v3 migration guide](docs/migration-v2-to-v3.md).

### Breaking changes
- **Invalid settings now throw** `ArgumentException` from `Next()` instead of returning an error
  message as the "password". Use `TryNext(out var password)` for a non-throwing path.
- **Minimum runtime is now .NET 8.** The package targets `net8.0` and `net10.0`; `netstandard2.0`
  has been dropped. Consumers on .NET Framework or other older runtimes should stay on the 2.x line.

### Security / correctness fixes
- Cryptographically secure RNG (`CryptoRandomSource`) with **unbiased** integer sampling
  (via `RandomNumberGenerator.GetInt32` — removes modulo bias).
- Fixed an off-by-one in length handling and removed the GUID-based shuffle in favour of a
  Fisher–Yates shuffle.
- Disposed/owned RNG lifecycle; removed dead code and the static RNG.
- Empty special-character sets are validated rather than silently producing weaker output.

### Added
- **Async APIs:** `NextAsync`, `GenerateAsync`.
- **Dependency injection:** `AddPasswordGenerator(...)` with code and `appSettings.json` binding
  (resolution order: code-configure > appSettings > default).
- **Presets:** `ForOwasp`, `ForNist`, `ForOtp`, `ForApiKey`, `ForEnvironmentName`, `ForPassphrase`.
- **Passphrases use the EFF Large Wordlist** (7,776 words, ~12.9 bits/word), replacing the small
  built-in list — a 6-word phrase is now ~77 bits. The list is © EFF under CC BY 3.0; see
  [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
- **Entropy-targeted passphrases:** `ForPassphraseWithEntropy(targetBits)` derives the word count
  to meet a target, and `ForPassphrase(..., minimumEntropyBits)` enforces an entropy floor.
- `EstimateEntropyBits()` is now part of the `IPasswordGenerator` interface.
- **Custom pools:** `WithCharacters(string)`, `WithAllAscii()`.
- **Quality controls:** `ExcludeAmbiguous()`, `RequireAtLeast(CharacterClass, count)`.
- **Entropy estimation:** `IEntropyEstimator` / `PoolEntropyEstimator` and `EstimateEntropyBits()`.
- **Batch API:** `Generate(count)` and a parameterless `Generate()` driven by
  `PasswordOptions.DefaultBatchCount`.

### Packaging
- Multi-targets `net8.0` and `net10.0`; nullable reference types enabled.
- Single source of version truth in the csproj (removed the stale `.nuspec`).
- `PackageIcon` + `PackageReadmeFile` (clears `NU5048`), SourceLink, deterministic build, and a
  `.snupkg` symbol package.

### Compatibility
- The v2 surface (`Next`, `NextGroup`, constructors, `IncludeX`, `LengthRequired`) is unchanged and
  continues to work, aside from the error-handling breaking change noted above.

## 2.1.0 and earlier

See the project history and the original review in
[`docs/archive/V3_REVIEW_AND_DOCUMENTATION.md`](docs/archive/V3_REVIEW_AND_DOCUMENTATION.md).
