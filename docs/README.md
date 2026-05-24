# PasswordGenerator — Documentation

This folder is the working reference for the package as it is **today** and the design we are
steering it toward in **v3**. Diagrams are written in [Mermaid](https://mermaid.js.org/) and render
directly on GitHub.

## How the docs fit together

```mermaid
flowchart LR
    subgraph Review["Review & verification"]
        A[V3_REVIEW_AND_DOCUMENTATION.md<br/>full review of v2.1.0]
        B[V3_VERIFICATION.md<br/>every issue re-checked vs current source]
    end
    subgraph Current["current-state/ — what we have"]
        C1[architecture.md]
        C2[generation-flow.md]
        C3[api-surface.md]
    end
    subgraph Target["v3-target/ — where we are going"]
        T1[architecture.md]
        T2[generation-flow.md]
        T3[api-surface.md]
        T4[configuration-and-di.md]
        T5[before-after.md]
        T6[roadmap.md]
        T7[implementation-plan.md]
    end
    A --> B --> Current
    Current --> Target
    T5 -. compares .-> Current
    T6 --> T7
```

## Reading order

1. **`V3_REVIEW_AND_DOCUMENTATION.md`** — the original full review (API, bugs, packaging, gaps).
2. **`V3_VERIFICATION.md`** — each issue re-checked against the current `master` source, with verdicts.
3. **`current-state/`** — diagrammed snapshot of the code as it runs today.
4. **`v3-target/`** — the proposed v3 design, diagrammed, with a before/after, a roadmap, and a
   phased **`implementation-plan.md`** (starts with installing the .NET SDK via bash).

## Conventions

- **Current state** describes `master` @ v2.1.0, `netstandard2.0`. Code references use
  `file:line` against that source.
- **v3 target** is a proposal for discussion, not yet implemented. Anything in `v3-target/` is
  subject to change as we agree the plan.
- Each "target" doc ends with a **Why this is better** note tied back to a verified issue.
