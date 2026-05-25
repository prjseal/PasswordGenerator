# Current State — Generation Flow (v2.1.0)

> **Historical.** This describes v2.1.0. The issues noted here are resolved in v3 — see the root
> [`CHANGELOG.md`](../../../CHANGELOG.md) and the [migration guide](../../migration-v2-to-v3.md).

How `Next()` produces a password today (`Password.cs:114-193`).

## `Next()` control flow

```mermaid
flowchart TD
    Start([Next called]) --> LenOK{"length in<br/>[Min, Max]?"}
    LenOK -- no --> ErrStr["return ERROR STRING:<br/>'Password length invalid...'"]
    LenOK -- yes --> Gen["GenerateRandomPassword(settings)"]
    Gen --> Valid{"PasswordIsValid?"}
    Valid -- yes --> RetPwd([return password])
    Valid -- no --> Attempts{"attempts <<br/>MaximumAttempts?"}
    Attempts -- yes --> Gen
    Attempts -- no --> TryAgain["return ERROR STRING:<br/>'Try again'"]

    classDef bad fill:#ffe6e6,stroke:#cc0000;
    class ErrStr,TryAgain bad;
```

**Verified problem (§5.1):** the two red nodes return human-readable **error strings in the same
`string` return slot as a real password**. A caller that does not special-case them will store an
error message as the user's password. There is no exception and no `TryNext`/result type.

## Inside `GenerateRandomPassword`

```mermaid
flowchart TD
    A["pool = settings.CharacterSet"] --> B["pool = Shuffle(pool)<br/>orderby Guid.NewGuid (non-crypto)"]
    B --> C["for each position 0..length-1"]
    C --> D["idx = GetRandomNumberInRange(0, len-1)<br/>= rnd % (len-1)  →  range 0..len-2"]
    D --> E["password[pos] = pool[idx]"]
    E --> F{"pos > 2 AND<br/>3 identical in a row?"}
    F -- yes --> G["pos-- (redo this position)"]
    F -- no --> H["next position"]
    G --> C
    H --> C

    classDef bad fill:#ffe6e6,stroke:#cc0000;
    class B,D,F bad;
```

Verified problems in this loop:
- **§5.5** — `Shuffle` is a non-crypto `Guid.NewGuid()` sort (redundant; randomness really comes from
  `GetRandomNumberInRange`).
- **§5.2 (off-by-one)** — `GetRandomNumberInRange(0, len-1)` computes `% (len-1)`, so the **top index
  is never selected**; the effective alphabet is one char short per password.
- **§5.3 (modulo bias)** — `rnd % n` over a full-range `Int32` is not uniform.
- **§5.4** — the "no 3 identical in a row" guard only starts at position > 2, so the **first three
  characters can be identical**.

## How validity is decided (`PasswordIsValid`, `Password.cs:208`)

```mermaid
flowchart LR
    P[password] --> L{"lower required?<br/>→ regex match"}
    P --> U{"upper required?<br/>→ regex match"}
    P --> N{"numeric required?<br/>→ regex match"}
    P --> S{"special required?<br/>→ any special char present"}
    P --> Len{"length in range?"}
    L & U & N & S & Len --> AND{{"all true?"}}
    AND -- yes --> OK([valid])
    AND -- no --> NO([invalid → retry])
```

**Verified problem (§5.8):** if `IncludeSpecial` is true but the custom special set is empty/whitespace,
`specialIsValid` stays `false` forever, so every attempt fails and `Next()` silently returns
`"Try again"`.

## Why this design is fragile

```mermaid
stateDiagram-v2
    [*] --> Configured
    Configured --> Generating: Next()
    Generating --> Generating: invalid (retry up to MaximumAttempts)
    Generating --> Success: valid password
    Generating --> FailureString: attempts exhausted
    Configured --> FailureString: length invalid
    note right of FailureString
        Failure is a magic STRING,
        not an exception. Caller may
        not notice. (§5.1)
    end note
    Success --> [*]
    FailureString --> [*]
```

The whole correctness contract hinges on probabilistic retry + string sentinels — the core thing v3
replaces (see `../../generation-flow.md`).
