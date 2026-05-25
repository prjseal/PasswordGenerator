# Current State — Public API Surface (v2.1.0)

> **Historical.** This describes v2.1.0. The issues noted here are resolved in v3 — see the root
> [`CHANGELOG.md`](../../../CHANGELOG.md) and the [migration guide](../../migration-v2-to-v3.md).

What a caller can do today, and how configuration is resolved.

## API map

```mermaid
flowchart TD
    subgraph Construct["Construction (6 constructors)"]
        c0["Password()"]
        c1["Password(int length)"]
        c2["Password(IPasswordSettings)"]
        c3["Password(bool l, u, n, s)"]
        c4["Password(bool l,u,n,s, int length)"]
        c5["Password(bool l,u,n,s, int length, int maxAttempts)"]
    end
    subgraph Fluent["Fluent builders (return this)"]
        f1["IncludeLowercase()"]
        f2["IncludeUppercase()"]
        f3["IncludeNumeric()"]
        f4["IncludeSpecial()"]
        f5["IncludeSpecial(string)"]
        f6["LengthRequired(int)"]
    end
    subgraph Generate["Generation"]
        g1["Next() : string"]
        g2["NextGroup(int) : IEnumerable~string~"]
    end
    Construct --> Fluent --> Generate
```

## Configuration resolution (today)

```mermaid
flowchart LR
    A["Constructor args<br/>or defaults"] --> S[(PasswordSettings)]
    B["Fluent Include*/LengthRequired"] --> S
    S --> G["Next()"]
    note1["First fluent call on a defaulted<br/>Password() clears the pool<br/>(StopUsingDefaults)"]
    B -.-> note1
```

There are only two sources: constructor arguments (or built-in defaults) and fluent calls. There is
**no** external configuration (`appSettings`), **no** DI, and **no** presets.

Defaults: all four classes on, length 16, `MaximumAttempts` 10000, length bounds 4–256, default
special set `!#$%&*@\` (8 chars).

## Key behaviour quirks (verified)

```mermaid
flowchart TD
    Q1["new Password().IncludeNumeric()"] --> R1["numeric ONLY, length 16<br/>(first fluent call clears defaults)"]
    Q2["Next() on bad config"] --> R2["returns an ERROR STRING (§5.1)"]
    Q3["NextGroup(n)"] --> R3["n passwords, NOT de-duplicated (§5.10)"]
    Q4["IncludeSpecial(empty string)"] --> R4["always 'Try again' (§5.8)"]
    classDef bad fill:#ffe6e6,stroke:#cc0000;
    class R2,R3,R4 bad;
```

## What the surface does NOT offer (verified gaps, §8)

| Missing today | Confirmed |
|---|---|
| `TryNext` / result type (failures are strings) | ✅ |
| Async API | ✅ |
| DI registration helper | ✅ |
| Presets (OWASP/NIST/OTP/passphrase/API-key/env-name) | ✅ |
| `appSettings` configuration | ✅ |
| First-class custom alphabet (`WithAllAscii`/`WithCharacters`) | ✅ (only hackable via `IncludeSpecial(string)`) |
| Exclude-ambiguous, per-class minimums, entropy estimate | ✅ |
| `netstandard2.0` + `net8.0` multi-target / nullable | ✅ (netstandard2.0 only) |

These gaps define the v3 surface in `../../api-surface.md`.
