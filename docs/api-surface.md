# Public API Surface

Keeps the familiar fluent feel; adds safety, presets, batch, async, and custom pools.

> The fluent builder is `IPassword` (there is no separate `IPasswordBuilder`/`Build()` split).
> `Password` implements both `IPassword` and the generation contract `IPasswordGenerator`.
> Passphrases return an `IPasswordGenerator` (`PassphraseGenerator`). See
> `archive/implementation-plan.md` for how the shipped surface diverged from the early proposal.

## API map

```mermaid
flowchart TD
    subgraph Entry["Entry points"]
        e1["new Password()"]
        e2["Password.ForOwasp()/ForOtp()/... (static presets)"]
        e3["inject IPasswordGenerator (DI)"]
    end
    subgraph Build["Fluent builder (IPassword)"]
        direction TB
        b1["IncludeLowercase/Uppercase/Numeric"]
        b2["IncludeSpecial(string)"]
        b3["WithAllAscii() / WithCharacters(string)"]
        b4["ExcludeAmbiguous()"]
        b5["RequireAtLeast(class, count)"]
        b6["LengthRequired(int)"]
    end
    subgraph Gen["Generation (IPasswordGenerator)"]
        g1["Next() : string  (throws on bad config)"]
        g2["TryNext(out string) : bool"]
        g3["NextAsync(ct) : Task~string~"]
        g4["Generate() / Generate(count)"]
        g5["GenerateAsync() / GenerateAsync(count, ct)"]
    end
    Entry --> Build --> Gen
    classDef good fill:#e6ffe6,stroke:#009900;
    class g2,g3,g4,g5,b3,b4,b5 good;
```

## Single vs batch (naming kept intentional)

```mermaid
flowchart LR
    N["Next() — ONE password<br/>(mirrors Random.Next())"]
    G["Generate(count) — MANY<br/>Generate() — DefaultBatchCount<br/>(bindable from appSettings)"]
    N -. same options .- G
```

`.Next()` is retained because the original API was modelled on `Random.Next()`. `.Generate()` is the
new batch-oriented entry: `Generate(count)` plus a parameterless `Generate()` that uses the
configurable `DefaultBatchCount` (bindable from appSettings). The `.Count(n)` chaining shape from the
early proposal was not added — there is no new return type.

## Presets → standards mapping

```mermaid
flowchart LR
    ForOwasp --> O["all printable ASCII, no forced composition"]
    ForNist --> Nn["NIST 800-63B aligned length/charset"]
    ForOtp --> Ot["short numeric, e.g. 4-6 digits"]
    ForPassphrase --> Pp["diceware word-list"]
    ForApiKey --> Ak["long, URL-safe charset"]
    ForEnvironmentName --> En["readable, memorable identifiers"]
```

Presets are static factory methods on `Password` (sugar over the fluent builder); any subsequent
fluent call still overrides them (resolution order is documented in `configuration-and-di.md`).

## Surfacing the broader purpose

The library is **not password-only**. The same surface generates OTPs, environment names, API keys,
and other identifiers — so the library deliberately keeps the per-class `Include*` methods and adds
`WithCharacters`/`WithAllAscii` rather than forcing OWASP composition or a global 12-char minimum.

## Deprecation / migration shape

```mermaid
flowchart TD
    Old["v2: new Password().Next() → string (maybe error)"] --> Mig["v3 migration"]
    Mig --> A["sync Next()/Generate() kept and fully supported<br/>(NOT obsoleted); async added alongside"]
    Mig --> B["error strings → exception / TryNext"]
    Mig --> C["direct new → optional IPasswordGenerator via DI"]
    Mig --> D["[Obsolete] PasswordGenerator/Settings REMOVED"]
    classDef warn fill:#fff0e6,stroke:#cc6600;
    class D warn;
```

> Sync methods are **not** marked `[Obsolete]`: generation is CPU-bound, so obsoleting sync in favour
> of async would be an anti-pattern and would spam every consumer with build warnings. Async exists
> for ergonomics and cancellation only.

**Why this is better:** every gap noted in the v2.1.0 review (`archive/current-state/api-surface.md`) is closed
(`TryNext`/async/DI/presets/appSettings/custom pools), failures become explicit, and existing single
`.Next()` users still work unchanged, giving a gentle upgrade path.
