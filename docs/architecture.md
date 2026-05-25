# Architecture

> Multi-targets `net8.0;net10.0`, nullable enabled. `IPassword` is the fluent builder (no
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
        RandomNumberGenerator.GetInt32
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

Key points:
- **`IRandomSource` abstraction** wraps the CSPRNG (unbiased `RandomNumberGenerator.GetInt32`). No
  `static`, injectable — a deterministic `IRandomSource` can be injected in unit tests — and the
  Guid-based `Shuffle` is gone in favour of Fisher–Yates.
- **`PasswordOptions`** is the DI config object, bindable from `IConfiguration`.
- **Presets** are static factory methods on `Password` that pre-fill the fluent builder.
- The `[Obsolete]` v2 wrappers from earlier proposals are not present.

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
    subgraph net8["net8.0"]
        b["RandomNumberGenerator.GetInt32"]
    end
    subgraph net10["net10.0"]
        c["RandomNumberGenerator.GetInt32"]
    end
    IRandomSource --> net8
    IRandomSource --> net10
```

Both targets use the same built-in `RandomNumberGenerator.GetInt32`, so `CryptoRandomSource` needs no
`#if` and exposes one uniform public surface. (`netstandard2.0`, which required a manual
rejection-sampling fallback, was dropped in v3 — see the [changelog](../CHANGELOG.md).)

**Why this is better:** removes the `static`/undisposed RNG, makes randomness unbiased and testable
(inject a deterministic `IRandomSource` in unit tests), and uses the fast, allocation-free built-in
crypto API on every supported runtime.
