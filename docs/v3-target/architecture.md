# v3 Target — Architecture (proposal)

> Proposed design for discussion. Multi-target `netstandard2.0;net8.0` (optionally `net10.0`),
> nullable enabled. Aligns with the adjusted plan in `../V3_VERIFICATION.md` §3.

## Target type relationships

```mermaid
classDiagram
    class IPasswordGenerator {
        <<interface>>
        +Next() string
        +TryNext(out string) bool
        +NextAsync(CancellationToken) Task
        +Generate(int count) IReadOnlyList
        +GenerateAsync(int count, CancellationToken) Task
    }
    class IPasswordBuilder {
        <<interface>>
        +IncludeLowercase() IPasswordBuilder
        +IncludeUppercase() IPasswordBuilder
        +IncludeNumeric() IPasswordBuilder
        +IncludeSpecial(string) IPasswordBuilder
        +WithAllAscii() IPasswordBuilder
        +WithCharacters(string) IPasswordBuilder
        +ExcludeAmbiguous() IPasswordBuilder
        +RequireAtLeast(class, count) IPasswordBuilder
        +LengthRequired(int) IPasswordBuilder
        +ForOwasp() IPasswordBuilder
        +ForOtp() IPasswordBuilder
        +ForPassphrase() IPasswordBuilder
        +Build() IPasswordGenerator
    }
    class PasswordOptions {
        +pools, length, minCounts
        +excludeAmbiguous
        +maxAttempts
        +bind from IConfiguration
    }
    class IRandomSource {
        <<interface>>
        +int NextInt(int maxExclusive)
        +void Fill(Span~byte~)
    }
    class CryptoRandomSource {
        uses RandomNumberGenerator.GetInt32
    }
    class IEntropyEstimator {
        <<interface>>
        +double Bits(string password)
    }

    IPasswordGenerator <|.. PasswordGenerator2
    IPasswordBuilder <|.. PasswordBuilder
    PasswordBuilder --> PasswordOptions : produces
    PasswordGenerator2 --> PasswordOptions : reads
    PasswordGenerator2 --> IRandomSource : uses
    IRandomSource <|.. CryptoRandomSource
    PasswordGenerator2 ..> IEntropyEstimator : optional
```

Key shifts from today:
- **`IRandomSource` abstraction** wraps the CSPRNG (unbiased `RandomNumberGenerator.GetInt32` on
  `net8.0`; rejection-sampling fallback on `netstandard2.0`). No `static`, disposable-aware,
  injectable. Fixes verified §5.2/§5.3/§5.6 and lets the Guid `Shuffle` be deleted (§5.5).
- **`PasswordOptions`** is the single config object, bindable from `IConfiguration`.
- **Presets** are builder methods that pre-fill `PasswordOptions`.
- The `[Obsolete]` v2 wrappers are **removed** in v3 (recommended in `../V3_VERIFICATION.md` §4).

## Target composition (with DI)

```mermaid
flowchart TD
    App["Consuming app"] -->|AddPasswordGenerator| DI["IServiceCollection"]
    DI --> Reg["registers IPasswordGenerator,<br/>IRandomSource, PasswordOptions"]
    App -->|inject| IPG["IPasswordGenerator"]
    App -->|or new directly| Builder["new PasswordBuilder()...Build()"]
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
    subgraph net8["net8.0 / net10.0 (modern)"]
        b["RandomNumberGenerator.GetInt32 / GetItems"]
    end
    IRandomSource --> ns
    IRandomSource --> net8
```

`#if` inside `CryptoRandomSource` selects the optimal API per target while keeping one public surface.

**Why this is better:** removes the `static`/undisposed RNG, makes randomness unbiased and testable
(inject a deterministic `IRandomSource` in unit tests), keeps .NET Framework users supported, and
gives modern consumers the fast built-in APIs — addressing verified issues §5.2, §5.3, §5.5, §5.6 and
gaps §8 (async/DI/multi-target) at the architecture level.
