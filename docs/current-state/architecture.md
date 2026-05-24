# Current State — Architecture (v2.1.0)

> **Historical.** This describes v2.1.0. The issues noted here are resolved in v3 — see the root
> [`CHANGELOG.md`](../../CHANGELOG.md) and the [migration guide](../v3-target/migration-v2-to-v3.md).

`master` @ v2.1.0 · target `netstandard2.0` · no third-party runtime dependencies.

## Type relationships

```mermaid
classDiagram
    class IPassword {
        <<interface>>
        +IncludeLowercase() IPassword
        +IncludeUppercase() IPassword
        +IncludeNumeric() IPassword
        +IncludeSpecial() IPassword
        +IncludeSpecial(string) IPassword
        +LengthRequired(int) IPassword
        +Next() string
        +NextGroup(int) IEnumerable~string~
    }
    class IPasswordSettings {
        <<interface>>
        +bool IncludeLowercase
        +bool IncludeUppercase
        +bool IncludeNumeric
        +bool IncludeSpecial
        +int PasswordLength
        +string CharacterSet
        +int MaximumAttempts
        +int MinimumLength
        +int MaximumLength
        +string SpecialCharacters
        +AddLowercase() IPasswordSettings
        +AddUppercase() IPasswordSettings
        +AddNumeric() IPasswordSettings
        +AddSpecial() IPasswordSettings
        +AddSpecial(string) IPasswordSettings
    }
    class Password {
        -static RandomNumberGenerator _rng
        +IPasswordSettings Settings
        +Next() string
        +NextGroup(int) IEnumerable~string~
        -GenerateRandomPassword(settings)$ string
        -GetRandomNumberInRange(min, max)$ int
        -PasswordIsValid(settings, pwd)$ bool
        -Shuffle(items)$ IEnumerable
        -GetRngCryptoSeed(rng)$ int
    }
    class PasswordSettings {
        +BuildCharacterSet(...)
        -StopUsingDefaults()
    }
    class PasswordGenerator {
        <<Obsolete>>
    }
    class PasswordGeneratorSettings {
        <<Obsolete>>
    }

    IPassword <|.. Password
    IPasswordSettings <|.. PasswordSettings
    Password o-- IPasswordSettings : Settings
    Password <|-- PasswordGenerator : inherits
    PasswordSettings <|-- PasswordGeneratorSettings : inherits
```

Notes:
- `PasswordGenerator` / `PasswordGeneratorSettings` are `[Obsolete]` back-compat wrappers. The five
  `CS0108` build warnings come from `PasswordGenerator` hiding `Password` methods without `new`.
- `_rng` is a **`static`** field on `Password` (`Password.cs:20`), reassigned in **every** constructor
  and never disposed (verified issue §5.6).
- `GetRngCryptoSeed` (`Password.cs:195`) is dead code still referencing the legacy
  `RNGCryptoServiceProvider` (verified issue §5.7).

## Runtime composition

```mermaid
flowchart TD
    Caller["Caller code"] -->|new Password / fluent| P[Password]
    P --> S[PasswordSettings<br/>character pools, length, attempts]
    P -->|reads CharacterSet| S
    P --> RNG["static RandomNumberGenerator (CSPRNG)"]
    P -->|orderby Guid.NewGuid| SH["Shuffle helper (non-crypto)"]
    classDef warn fill:#ffe6e6,stroke:#cc0000;
    class SH warn;
```

The output's randomness comes from the CSPRNG via `GetRandomNumberInRange` (`Password.cs:189`). The
`Shuffle` helper (`Password.cs:247-249`) reshuffles the pool first using `Guid.NewGuid()` — a
non-crypto, non-uniform sort that is now **redundant** (verified issue §5.5, reclassified as cleanup).

## Packaging / build snapshot

- Single packable project `PasswordGenerator.csproj`, version `2.1.0`.
- Stale `PasswordGenerator.nuspec` declares `2.0.5` (verified issue §7).
- `dotnet pack` emits `NU5048` (deprecated `PackageIconUrl`) and "missing readme".
- Tests target EOL `netcoreapp2.2` (pulls vulnerable `Microsoft.NETCore.App 2.2.0`).
