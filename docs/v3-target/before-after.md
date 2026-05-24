# v3 Target — Before / After (proposal)

Side-by-side of the things that change most, each tied to a verified issue.

## 1. Failure handling

```mermaid
flowchart LR
    subgraph Before["v2.1.0 (§5.1)"]
        b1["pwd.Next()"] --> b2["string — might be<br/>'Try again' or<br/>'Password length invalid...'"]
        b2 --> b3["caller may store<br/>an ERROR as a password"]
    end
    subgraph After["v3"]
        a1["gen.Next()"] --> a2["valid password<br/>OR throws ArgumentException"]
        a1b["gen.TryNext(out pwd)"] --> a2b["bool + real password"]
    end
    classDef bad fill:#ffe6e6,stroke:#cc0000;
    classDef good fill:#e6ffe6,stroke:#009900;
    class b2,b3 bad;
    class a2,a2b good;
```

```csharp
// Before — silent footgun
var pwd = new Password(3).Next();          // = "Password length invalid. Must be between 4 and 256..."

// After — explicit
try { var pwd = gen.Next(); }              // throws ArgumentException for length 3
catch (ArgumentException ex) { /* handle */ }
if (gen.TryNext(out var p)) { /* use p */ }
```

## 2. Randomness & character selection

```mermaid
flowchart LR
    subgraph Before2["v2.1.0"]
        x1["Guid.NewGuid() shuffle (§5.5)"] --> x2["pick rnd % (len-1)<br/>top index never used (§5.2)<br/>modulo bias (§5.3)"]
    end
    subgraph After2["v3"]
        y1["IRandomSource (CSPRNG)"] --> y2["GetInt32 / rejection sampling<br/>uniform, full range"]
        y2 --> y3["crypto Fisher-Yates shuffle"]
    end
    classDef bad fill:#ffe6e6,stroke:#cc0000;
    classDef good fill:#e6ffe6,stroke:#009900;
    class x1,x2 bad;
    class y1,y2,y3 good;
```

## 3. Guaranteeing required character classes

```mermaid
flowchart LR
    subgraph Before3["v2.1.0"]
        g1["generate random"] --> g2["validate"] --> g3{"ok?"}
        g3 -- no --> g1
        g3 -- "no, 10000x" --> g4["'Try again' (§5.1)"]
    end
    subgraph After3["v3"]
        h1["seed one of each<br/>required class"] --> h2["fill + crypto-shuffle"] --> h3["valid by construction"]
    end
    classDef bad fill:#ffe6e6,stroke:#cc0000;
    classDef good fill:#e6ffe6,stroke:#009900;
    class g4 bad;
    class h3 good;
```

## 4. Configuration & wiring

| Concern | v2.1.0 | v3 |
|---|---|---|
| Sources | constructor args + fluent only | fluent **>** appSettings **>** default |
| DI | none | opt-in `AddPasswordGenerator(...)` (+ `IConfiguration` overload) |
| RNG ownership | `static`, reassigned per ctor, never disposed (§5.6) | injected `IRandomSource`, disposable-aware |
| Presets | none | `ForOwasp/ForNist/ForOtp/ForPassphrase/ForApiKey/ForEnvironmentName` |

## 5. Targets, tests, packaging

```mermaid
flowchart LR
    subgraph BeforeP["v2.1.0"]
        p1["netstandard2.0 only"]
        p2["tests on EOL netcoreapp2.2<br/>(vulnerable 2.2.0)"]
        p3["stale nuspec 2.0.5, NU5048,<br/>no readme in package"]
        p4["5x CS0108 from obsolete wrappers"]
    end
    subgraph AfterP["v3"]
        q1["netstandard2.0 + net8.0 (+net10.0)"]
        q2["tests on net8.0, NUnit 4<br/>+ uniqueness/entropy/edge cases"]
        q3["clean pack: PackageReadmeFile,<br/>PackageIcon, SourceLink, snupkg"]
        q4["obsolete wrappers removed → no CS0108"]
        q5["BenchmarkDotNet numbers in release notes"]
    end
    classDef good fill:#e6ffe6,stroke:#009900;
    class q1,q2,q3,q4,q5 good;
```

## Net effect

```mermaid
mindmap
  root((v3 better))
    Safety
      exceptions not strings
      TryNext
      guaranteed classes
    Correctness
      unbiased CSPRNG
      no off-by-one
      no modulo bias
    Reach
      multi-target
      nullable
      DI + appSettings
    Capability
      presets
      custom pools / WithAllAscii
      batch Generate + async
    Trust
      modern tests
      benchmarks in release notes
      clean packaging
```
