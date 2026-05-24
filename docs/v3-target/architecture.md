# v3 Target — Architecture

> Multi-target `netstandard2.0;net8.0`, nullable enabled. Aligns with the adjusted plan in
> `../V3_VERIFICATION.md` §3. As shipped, the existing `IPassword` remains the fluent builder (no
> separate `IPasswordBuilder`/`Build()`); `Password` implements both `IPassword` and the generation
> contract `IPasswordGenerator`.

## Type relationships

```mermaid
classDiagram
    class IPasswordGenerator {
        <<interface>>
        +Next() string
        +TryNext(out string) bool
        +NextAsync(CancellationToken) Task
        +Generate() IReadOnlyList
        +Generate(int count) IReadOnlyList
        +GenerateAsync(CancellationToken) Task
        +GenerateAsync(int count, CancellationToken) Task
    }
    class IPassword {
        <<interface>>
        +IncludeLowercase() IPassword
        +IncludeUppercase() IPassword
        +IncludeNumeric() IPassword
        +IncludeSpecial(string) IPassword
        +WithAllAscii() IPassword
        +WithCharacters(string) IPassword
        +ExcludeAmbiguous() IPassword
        +RequireAtLeast(class, count) IPassword
        +LengthRequired(int) IPassword
        +Next() string
        +TryNext(out string) bool
        +NextGroup(int) IEnumerable
    }
    class Password {
        +static ForOwasp/ForNist/ForOtp() IPassword
        +static ForApiKey/ForEnvironmentName() IPassword
        +static ForPassphrase() IPasswordGenerator
        +EstimateEntropyBits() double
    }
    class PasswordOptions {
        +IncludeLowercase/Uppercase/Numeric/Special
        +SpecialCharacters, Length
        +ExcludeAmbiguous, DefaultBatchCount
        +bind from IConfiguration
    }
    class IRandomSource {
        <<interface>>
        +int NextInt(int maxExclusive)
    }
    class CryptoRandomSource {
        GetInt32 on net8, rejection sampling on netstandard2.0
    }
    class IEntropyEstimator {
        <<interface>>
        +double EstimateBits(IPasswordSettings)
    }

    IPassword <|.. Password
    IPasswordGenerator <|.. Password
    IPasswordGenerator <|.. PassphraseGenerator
    Password --> IRandomSource : uses
    PasswordOptions ..> Password : configures (DI)
    IRandomSource <|.. CryptoRandomSource
    IEntropyEstimator <|.. PoolEntropyEstimator
    Password ..> PoolEntropyEstimator : EstimateEntropyBits
```

Key shifts from today:
- **`IRandomSource` abstraction** wraps the CSPRNG (unbiased `RandomNumberGenerator.GetInt32` on
  `net8.0`; rejection-sampling fallback on `netstandard2.0`). No `static`, disposable-aware,
  injectable. Fixes verified §5.2/§5.3/§5.6 and lets the Guid `Shuffle` be deleted (§5.5).
- **`PasswordOptions`** is the DI config object, bindable from `IConfiguration`.
- **Presets** are static factory methods on `Password` that pre-fill the fluent builder.
- The `[Obsolete]` v2 wrappers are **removed** in v3 (recommended in `../V3_VERIFICATION.md` §4).

## Target composition (with DI)

```mermaid
flowchart TD
    App["Consuming app"] -->|AddPasswordGenerator| DI["IServiceCollection"]
    DI --> Reg["registers IPasswordGenerator,<br/>IRandomSource, PasswordOptions"]
    App -->|inject| IPG["IPasswordGenerator"]
    App -->|or new directly| Builder["new Password()..."]
    IPG --> OPT[PasswordOptions]
    Builder --> OPT
    IPG --> RNG["IRandomSource → CryptoRandomSource"]
    Builder --> RNG
    classDef good fill:#e6ffe6,stroke:#009900;
    class RNG,Reg good;
```

The fluent API behaves **identically** whether the instance is `new`'d or resolved from DI — the DI
registration is only responsible for wiring `IRandomSource` and default `PasswordOptions`.

## Multi-targeting strategy

```mermaid
flowchart LR
    subgraph ns["netstandard2.0 (broad reach: .NET Framework, Umbraco)"]
        a["manual rejection sampling"]
    end
    subgraph net8["net8.0 (modern)"]
        b["RandomNumberGenerator.GetInt32"]
    end
    IRandomSource --> ns
    IRandomSource --> net8
```

`#if` inside `CryptoRandomSource` selects the optimal API per target while keeping one public surface.

**Why this is better:** removes the `static`/undisposed RNG, makes randomness unbiased and testable
(inject a deterministic `IRandomSource` in unit tests), keeps .NET Framework users supported, and
gives modern consumers the fast built-in APIs — addressing verified issues §5.2, §5.3, §5.5, §5.6 and
gaps §8 (async/DI/multi-target) at the architecture level.
