# v3 Target — Roadmap (proposal)

Tiered delivery from the adjusted plan in `../V3_VERIFICATION.md` §3. Sequencing only — not committed
dates.

## Tiers as phases

```mermaid
flowchart TD
    T1["Tier 1 — Correctness & Security<br/>exceptions+TryNext · unbiased CSPRNG · delete Guid shuffle<br/>· guarantee classes · fix static RNG · empty-special guard"]
    T2["Tier 2 — Modernisation<br/>multi-target+nullable · async+[Obsolete] sync · opt-in DI<br/>· BenchmarkDotNet · packaging hygiene · tests→net8/NUnit4"]
    T3["Tier 3 — New Features<br/>WithAllAscii/WithCharacters · presets · appSettings<br/>· Generate batch · exclude-ambiguous · min-counts · entropy"]
    T4["Tier 4 — Documentation<br/>v2→v3 migration guide · broader-purpose docs · OWASP/NIST mapping"]
    T1 --> T2 --> T3 --> T4
    classDef t1 fill:#ffe6e6,stroke:#cc0000;
    classDef t2 fill:#fff5e6,stroke:#cc6600;
    classDef t3 fill:#e6f0ff,stroke:#0066cc;
    classDef t4 fill:#e6ffe6,stroke:#009900;
    class T1 t1;
    class T2 t2;
    class T3 t3;
    class T4 t4;
```

## Indicative sequencing

```mermaid
gantt
    title v3 indicative sequencing (relative, not dated)
    dateFormat  X
    axisFormat  %s
    section Tier 1 Correctness
    IRandomSource + unbiased selection      :t1a, 0, 3
    Exceptions + TryNext                     :t1b, 0, 2
    Guarantee classes (seed+shuffle)         :t1c, after t1a, 2
    Remove static RNG + dead code            :t1d, after t1a, 1
    section Tier 2 Modernisation
    Multi-target + nullable                  :t2a, after t1c, 2
    Async plus Obsolete sync                 :t2b, after t2a, 2
    Opt-in DI + appSettings bind             :t2c, after t2a, 2
    Tests net8 + NUnit4 + BenchmarkDotNet    :t2d, after t1c, 3
    Packaging hygiene                        :t2e, after t2a, 1
    section Tier 3 Features
    Custom pools + WithAllAscii              :t3a, after t2c, 2
    Presets                                  :t3b, after t3a, 2
    Generate batch + uniqueness              :t3c, after t2b, 2
    Exclude-ambiguous + min-counts + entropy :t3d, after t3a, 3
    section Tier 4 Docs
    Migration guide + standards mapping      :t4a, after t3b, 2
```

## Dependency rationale

```mermaid
flowchart LR
    RNG["IRandomSource"] --> Classes["guarantee classes"]
    RNG --> Multi["multi-target"]
    Multi --> Async["async"]
    Multi --> DI["DI + appSettings"]
    DI --> Presets["presets"]
    Async --> Batch["Generate batch"]
    Presets --> Docs["migration guide"]
```

`IRandomSource` is the keystone: the correctness fixes, multi-targeting, and testability all build on
it, so it lands first.

## Decision gates (resolve before/within the tier)

```mermaid
flowchart TD
    D1{"Drop [Obsolete] v2 wrappers?"} -->|recommended: yes| G1["clears 5x CS0108; Tier 2"]
    D2{"Min target?"} -->|recommended: netstandard2.0 + net8.0| G2["Tier 2"]
    D3{"IConfiguration DI overload?"} -->|recommended: yes| G3["Tier 2 DI"]
    classDef q fill:#fff5e6,stroke:#cc6600;
    class D1,D2,D3 q;
```

See `../V3_VERIFICATION.md` §4 for the reasoning behind each recommendation.

## Release-note discipline

Every v3.x release includes comparative **BenchmarkDotNet** numbers (sync vs async; batch sizes 1 /
100 / 1000 / 10000; allocations) so performance trends are visible across versions.
