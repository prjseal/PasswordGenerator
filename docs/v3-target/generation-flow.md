# v3 Target — Generation Flow

Replaces probabilistic retry + string sentinels with **deterministic construction + exceptions**.

## Target `Next()` / `TryNext()` flow

```mermaid
flowchart TD
    Start([Next / TryNext]) --> Cfg{"options valid?<br/>(pools non-empty,<br/>length ≥ sum of minimums,<br/>length in range)"}
    Cfg -- no, Next() --> Throw["throw ArgumentException<br/>(clear message)"]
    Cfg -- no, TryNext() --> RetFalse([return false])
    Cfg -- yes --> Seed["Step 1: place one char from<br/>each REQUIRED class<br/>(satisfies minimum counts)"]
    Seed --> Fill["Step 2: fill remaining positions<br/>from the full pool"]
    Fill --> ShuffleC["Step 3: crypto-shuffle (Fisher-Yates<br/>via IRandomSource)"]
    ShuffleC --> Done([return password])

    classDef good fill:#e6ffe6,stroke:#009900;
    classDef bad fill:#fff0e6,stroke:#cc6600;
    class Seed,Fill,ShuffleC,Done good;
    class Throw bad;
```

**What changed vs today (`../current-state/generation-flow.md`):**
- No retry loop, no `MaximumAttempts` gamble, **no `"Try again"` string**. Required classes are
  *guaranteed* by construction (fixes the probabilistic guarantee gap, §8).
- Invalid configuration **throws** (`Next()`) or returns `false` (`TryNext`) — never a fake password
  (fixes §5.1 and §5.8).
- Selection uses unbiased `IRandomSource.NextInt(maxExclusive)` — no `% (len-1)` off-by-one, no
  modulo bias (fixes §5.2, §5.3).
- Shuffle is a real crypto Fisher–Yates, replacing `orderby Guid.NewGuid()` (fixes/cleans §5.5).

## Deterministic class-seeding (the core idea)

```mermaid
flowchart LR
    R["RequireAtLeast: 1 lower, 1 upper, 2 digits, 1 special"] --> Place["place those 5 chars first"]
    Place --> Rest["fill length-5 from full pool"]
    Rest --> Shuf["crypto Fisher-Yates shuffle"]
    Shuf --> Out["valid by construction —<br/>no validate-and-retry needed"]
    classDef good fill:#e6ffe6,stroke:#009900;
    class Out good;
```

## Async path (`NextAsync` / `GenerateAsync`)

```mermaid
sequenceDiagram
    participant App
    participant Gen as IPasswordGenerator
    participant RNG as IRandomSource (CSPRNG)
    App->>Gen: GenerateAsync(count: 1000, ct)
    loop count
        Gen->>RNG: NextInt(...) (sync, fast)
        RNG-->>Gen: index
    end
    Gen-->>App: Task<IReadOnlyList<string>>
    Note over App,Gen: sync Next()/Generate() still exist<br/>and are fully supported (NOT obsoleted)
```

> Note: generation is CPU-bound, so async mainly helps large-batch ergonomics and cancellation, not
> raw throughput — the **BenchmarkDotNet** suite (plan §10/§12) exists to prove where async actually
> pays off, with numbers published in every release note.

## Failure contract — before vs after

```mermaid
stateDiagram-v2
    state "v2.1.0 (today)" as Old {
        [*] --> RetryLoop
        RetryLoop --> OKo: valid
        RetryLoop --> StrFail: attempts exhausted → 'Try again' STRING
    }
    state "v3 (target)" as New {
        [*] --> Validate
        Validate --> BuildOK: build guarantees validity
        Validate --> Throw: invalid config → exception / false
    }
```

**Why this is better:** failure is impossible to ignore (exception or `bool`), output is always a
real password, randomness is unbiased and fully covered by deterministic-RNG unit tests, and the
slowest part of the old design (validate-and-retry) is gone.
