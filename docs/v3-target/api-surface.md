# v3 Target — Public API Surface (proposal)

Keeps the familiar fluent feel; adds safety, presets, batch, async, and custom pools.

## Target API map

```mermaid
flowchart TD
    subgraph Entry["Entry points"]
        e1["new PasswordBuilder()"]
        e2["inject IPasswordGenerator (DI)"]
    end
    subgraph Build["Fluent builder (IPasswordBuilder)"]
        direction TB
        b1["IncludeLowercase/Uppercase/Numeric"]
        b2["IncludeSpecial(string)"]
        b3["WithAllAscii() / WithCharacters(string)"]
        b4["ExcludeAmbiguous()"]
        b5["RequireAtLeast(class, count)"]
        b6["LengthRequired(int)"]
        b7["Presets: ForOwasp/ForNist/ForOtp/<br/>ForPassphrase/ForApiKey/ForEnvironmentName"]
    end
    subgraph Gen["Generation (IPasswordGenerator)"]
        g1["Next() : string  (throws on bad config)"]
        g2["TryNext(out string) : bool"]
        g3["NextAsync(ct) : Task~string~"]
        g4["Generate(count) / Generate().Count(n)"]
        g5["GenerateAsync(count, ct)"]
    end
    Entry --> Build --> Gen
    classDef good fill:#e6ffe6,stroke:#009900;
    class g2,g3,g4,g5,b3,b4,b5,b7 good;
```

## Single vs batch (naming kept intentional)

```mermaid
flowchart LR
    N["Next() — ONE password<br/>(mirrors Random.Next())"]
    G["Generate(count) — MANY<br/>Generate().Count(10)<br/>count from appSettings if unset"]
    N -. same options .- G
```

`.Next()` is retained because the original API was modelled on `Random.Next()`. `.Generate()` is the
new batch-oriented entry with count overloads, `.Count(n)` chaining, and an `appSettings` default.

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

Presets are sugar over `PasswordOptions`; any subsequent fluent call still overrides them
(resolution order is documented in `configuration-and-di.md`).

## Surfacing the broader purpose

The library is **not password-only**. The same surface generates OTPs, environment names, API keys,
and other identifiers — so v3 deliberately keeps the per-class `Include*` methods and adds
`WithCharacters`/`WithAllAscii` rather than forcing OWASP composition or a global 12-char minimum.

## Deprecation / migration shape

```mermaid
flowchart TD
    Old["v2: new Password().Next() → string (maybe error)"] --> Mig["v3 migration"]
    Mig --> A["sync Next()/Generate() kept but [Obsolete] → async"]
    Mig --> B["error strings → exception / TryNext"]
    Mig --> C["direct new → optional IPasswordGenerator via DI"]
    Mig --> D["[Obsolete] PasswordGenerator/Settings REMOVED"]
    classDef warn fill:#fff0e6,stroke:#cc6600;
    class D warn;
```

**Why this is better:** every verified gap in `../current-state/api-surface.md` is closed
(`TryNext`/async/DI/presets/appSettings/custom pools), failures become explicit, and existing single
`.Next()` users still work (with an obsolete-hint nudge), giving a gentle upgrade path.
