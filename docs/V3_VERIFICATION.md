# PasswordGenerator v3 — Verification Report

> Companion to `V3_REVIEW_AND_DOCUMENTATION.md` and the v3 Planning Addendum.
> This document does the verification the addendum asked for: every item from the original
> review's bug list (§5) and feature gaps (§8) was re-checked against the **current source**,
> and marked **Confirmed**, **Already Fixed**, or **Partially Fixed** with a code reference.
>
> Verification date: 2026-05-24.
> **Code state verified:** the source files on this branch are byte-identical to `origin/master`
> (`git diff origin/master -- PasswordGenerator/ PasswordGenerator.Tests/` is empty). The latest
> source commit is `36f2b58` *"removed usage of RNG Crypto Provider and replaced with
> RandomNumberGenerator"*. So the code reviewed here **is** the current default-branch state.

---

## 0a. Which branch is current? (`master` vs `dev/v2`)

`master` is the **most up-to-date** branch. `dev/v2` is the **older** v2.0.0 line — it has
diverged but is behind master on everything substantive:

| Aspect | `dev/v2` (v2.0.0, `netstandard1.2`) | `master` (v2.1.0, `netstandard2.0`) |
|---|---|---|
| Randomness | `new Random()` — **insecure** (`Password.cs:148` on dev/v2) | `RandomNumberGenerator` CSPRNG (`Password.cs:189`) |
| Guid shuffle | present (`:204`) | **also present** (`:247-249`) |
| Length range | 8–128 | 4–256 |
| Custom special chars | absent | present (`IncludeSpecial(string)`) |
| Bug-fix tests | absent | present (`077b798`) |

`dev/v2` carries a handful of commits master lacks, but they are all non-substantive
(readme/logo/nuspec/appveyor tweaks + an early "passwordservice" refactor). **Verify against
`master`** — which equals this branch's source.

Key implication: the **Guid shuffle exists on BOTH branches and was never removed anywhere**, so the
"already fixed" recollection does not hold on any branch (see §0). What was actually fixed — only on
master — was `Random` → `RNGCryptoServiceProvider` → `RandomNumberGenerator` for *selection*.

---

## 0. Headline correction (read this first)

The addendum states the **Guid-based shuffle (original §5.5) is "ALREADY FIXED — remove from v3
scope."** That is **not** what the code shows.

- **What commit `36f2b58` actually changed:** the *character-selection* randomness. `GetRandomNumberInRange`
  now draws from the CSPRNG — `_rng.GetBytes(data)` at `Password.cs:189`, where
  `_rng = RandomNumberGenerator.Create()`. ✅ This part of the author's recollection is correct: the
  **output's randomness now comes from a CSPRNG**, not from `Random` or from Guids.
- **What was NOT changed:** the `Shuffle` helper still uses `orderby Guid.NewGuid()` —
  `Password.cs:247-249`, called at `Password.cs:163`. The Guid shuffle is **still in the code**.

**Net verdict: Partially Fixed.** The Guid shuffle is no longer the source of the password's
randomness (so it is not a meaningful security hole anymore), but it is still present as
**redundant, non-uniform dead-weight** that reshuffles the pool before the CSPRNG indexes into it.
Recommendation: **keep a small cleanup task in v3** to delete `Shuffle` (and its call site) — do
**not** drop it from scope entirely. Selection via `GetRandomNumberInRange` alone already provides
the randomness; the shuffle adds nothing but a non-crypto code path.

---

## 1. Bug list (§5) — verification

| # | Original issue | Verdict | Evidence (current code) |
|---|---|---|---|
| 5.1 | `Next()` returns error text as a password (`"Try again"`, length message) | **Confirmed** | `Password.cs:119-120` (length message) and `Password.cs:131` (`… ? password : "Try again"`). No exception, no `TryNext`, no result type. |
| 5.2 | Off-by-one: top index of the pool never selected | **Confirmed** | `Password.cs:170` calls `GetRandomNumberInRange(0, characterSetLength - 1)`; `Password.cs:192` computes `… % (max - min)` = `% (characterSetLength - 1)` → range `0 … len-2`. Empirically reproduced earlier (index 9 never produced for a 10-element range). |
| 5.3 | Modulo bias (non-uniform selection) | **Confirmed** | `Password.cs:192` `randomNumber % (max - min)` over a full-range `Int32`. Should be rejection sampling / `RandomNumberGenerator.GetInt32`. |
| 5.4 | "Max 2 identical in a row" rule mis-guarded; first 3 chars can be identical | **Confirmed** | `Password.cs:173` guard is `characterPosition > maximumIdenticalConsecutiveChars` (i.e. `> 2`), so the check only starts at position 3. |
| 5.5 | Non-cryptographic, non-uniform Guid shuffle | **Partially Fixed** | Output randomness now from CSPRNG (`Password.cs:189`), but Guid shuffle still present at `Password.cs:247-249` (used at `:163`). See §0. Reclassify as **cleanup**, not security. |
| 5.6 | `_rng` is `static`, reassigned in every ctor, never disposed | **Confirmed** | `static` field `Password.cs:20`; reassigned in all six constructors (`:28, :35, :43, :51, :60, :69`); never disposed (it is `IDisposable`). |
| 5.7 | Dead code `GetRngCryptoSeed` referencing `RNGCryptoServiceProvider` | **Confirmed** | `Password.cs:195-200`. Unused; still references the legacy provider the commit message claimed to remove. |
| 5.8 | `IncludeSpecial` with empty/whitespace custom set silently never validates → `"Try again"` | **Confirmed** | `PasswordIsValid` `Password.cs:221-229`: `specialIsValid` only becomes `true` when `IncludeSpecial && !IsNullOrWhiteSpace(SpecialCharacters)` and a match is found; otherwise stays `false`. |
| 5.9 | `Math.Abs(int.MinValue)` overflow | **Confirmed (latent, NOT reachable)** | `Password.cs:192` applies `% (max - min)` *before* `Math.Abs`, bounding the operand. Standalone overflow verified earlier, but not reachable here. Keep in mind for the rewrite. |
| 5.10 | `NextGroup` does not de-duplicate | **Confirmed** | `Password.cs:138-149` simply loops `Next()` and adds to a `List<string>`. Test `…ShouldReturn10DifferentPasswords` only asserts count. |

### Documentation defects (§6) — still present
- Readme still says length "Must be between 8 and 128"; code enforces **4 and 256**
  (`PasswordSettings.cs:14-15`). **Confirmed.**
- C# samples still fenced as ```javascript```. **Confirmed.**
- `IncludeSpecial(string)` still absent from the obsolete `PasswordGenerator` wrapper. **Confirmed.**

### Packaging (§7) — still present
- `PasswordGenerator.csproj:5,21` = `2.1.0`; stale `PasswordGenerator.nuspec:5` = `2.0.5`. **Confirmed.**
- `PackageIconUrl` deprecation (`NU5048`) and missing `PackageReadmeFile` confirmed by `dotnet pack`
  output during CI work. **Confirmed.**
- The 5 `CS0108` member-hiding warnings on the obsolete wrapper are still emitted. **Confirmed.**

---

## 2. Feature gaps (§8) — verification

All confirmed **absent** in current code (no hidden implementations found across the whole
`PasswordGenerator/` project — the only public surface is `IPassword`/`Password`/`IPasswordSettings`/
`PasswordSettings` plus the two obsolete wrappers):

| Gap | Verdict | Note |
|---|---|---|
| Passphrase / word-list (diceware) | **Confirmed gap** | No word list, no passphrase path. |
| Exclude ambiguous characters (`0/O`, `1/l/I`) | **Confirmed gap** | No such option. |
| Per-class minimum counts (e.g. "≥2 digits") | **Confirmed gap** | Only presence is checked, not counts. |
| Guarantee at least one of each included class | **Confirmed gap** | Probabilistic: relies on the generate-and-validate retry loop (`Password.cs:124-131`), hence `"Try again"`. |
| Entropy / strength estimate | **Confirmed gap** | None. |
| Pronounceable / memorable mode | **Confirmed gap** | None. |
| `Span<char>` / low-allocation API | **Confirmed gap** | Uses `string`/`char[]`/LINQ throughout. |
| Async API | **Confirmed gap** | None. |
| DI helper (`AddPasswordGenerator()`) | **Confirmed gap** | None. |
| `TryNext` / `Result` pattern | **Confirmed gap** | Failures are magic strings (§5.1). |
| Custom full alphabet beyond special chars | **Partially achievable today** | No first-class API, but a caller *can* abuse `IncludeSpecial("…")` with the other classes off to supply an arbitrary pool (`PasswordSettings.cs:79-86`). v3 should add a clean `WithCharacters(...)`/`WithAllAscii()`. |
| `NextGroup` uniqueness | **Confirmed gap** | Dup of 5.10. |
| `net6`/`net8` target | **Confirmed gap** | `PasswordGenerator.csproj:4` is `netstandard2.0` only. |
| Nullable reference annotations | **Confirmed gap** | No `<Nullable>enable</Nullable>` in the csproj. |

---

## 3. Adjusted v3 plan (reconciling the original review + the addendum + this verification)

### Tier 1 — Correctness & security
1. **Replace error-string returns** with exceptions + a `TryNext`/`PasswordResult` pattern. (5.1) — *Confirmed, keep.*
2. **Fix selection: unbiased rejection sampling** (`RandomNumberGenerator.GetInt32` on modern TFMs;
   manual rejection on `netstandard2.0`). Fixes 5.2 + 5.3 in one change. — *Confirmed, keep.*
3. **Delete the Guid `Shuffle`** (and its call site at `Password.cs:163`). — **Keep as a small
   cleanup task** (the addendum's "remove from scope" is based on an inaccurate belief that the code
   was already removed — it was not; see §0). Reclassified from "security" to "cleanup".
4. **Guarantee included classes deterministically** (seed one of each required class, then fill &
   shuffle with the CSPRNG) → removes `"Try again"` and the `MaximumAttempts` retry loop. — *Keep.*
   - Addendum nuance: retry behaviour, where it remains, must be **configurable** (fluent /
     appSettings / library default) and must **throw** on exhaustion, never return a string.
5. **Remove dead code** `GetRngCryptoSeed` (5.7) and **fix the `static`/undisposed `_rng`** design
   (5.6) — make the RNG an instance field (or use the static `RandomNumberGenerator.Fill`/`GetInt32`
   static APIs and hold no field at all). — *Confirmed, keep.*
6. **Fix the empty-custom-special-set trap** (5.8) — validate the configuration up front and throw a
   clear exception instead of silently failing. — *Confirmed, keep.*

### Tier 2 — Modernisation
7. **Multi-target** `netstandard2.0;net8.0` (see open-question recommendation below); add nullable
   annotations.
8. **Async API**: add `NextAsync()` / `GenerateAsync()`; mark sync methods `[Obsolete]` pointing to
   async equivalents (gentle deprecation). *(addendum)*
9. **DI support**: `services.AddPasswordGenerator()` extension, opt-in (not auto-registered); wires up
   the RNG; fluent API behaves identically whether `new`'d or resolved. *(addendum, confirmed gap)*
10. **BenchmarkDotNet** project alongside tests (sync vs async, batch sizes 1/100/1000/10000,
    allocations); include comparative benchmark numbers in every release note going forward. *(addendum)*
11. **Packaging hygiene**: delete/regenerate the stale nuspec, add `<PackageReadmeFile>`, replace
    `PackageIconUrl` with `<PackageIcon>` (clears `NU5048`), add SourceLink + deterministic build +
    `snupkg`. Fix the 5 `CS0108` warnings (or drop the obsolete wrappers — see open questions).
12. **Tests**: retarget to `net8.0` + NUnit 4 (current `netcoreapp2.2` is EOL and pulls vulnerable
    `Microsoft.NETCore.App 2.2.0`); add uniqueness, entropy, and edge-case tests. This also makes the
    AppVeyor `dotnet test` step reliable.

### Tier 3 — New features *(all confirmed absent today)*
13. Fluent character-pool control incl. `.WithAllAscii()` / `WithCharacters(...)`; keep existing
    `Include*` methods (library is also used for OTPs, env names, API keys). **Do not** impose a
    global 12-char minimum.
14. Use-case / compliance presets: `.ForOwasp()`, `.ForNist()`, `.ForHipaa()`, `.ForPciDss()`,
    `.ForOtp()`, `.ForPassphrase()`, `.ForApiKey()`, `.ForEnvironmentName()`.
15. `appSettings` configuration with resolution order fluent > appSettings > library default
    (separate, opt-in step).
16. `.Generate()` / `.GenerateAsync()` batch API (count overloads + `.Count(n)` chaining +
    appSettings default); keep `.Next()` for single (mirrors `Random.Next()`).
17. Exclude-ambiguous option, per-class minimum counts, entropy estimate, `NextGroup`/`Generate`
    uniqueness option.

### Tier 4 — Documentation *(addendum)*
18. v2→v3 migration guide (direct→DI, sync→async, error-string→exceptions, presets/appSettings).
19. Clarify broader purpose (OTPs, env names, API keys), document preset↔standard mapping with
    OWASP/NIST links.

---

## 4. Recommendations on the open questions

1. **Drop the `[Obsolete] PasswordGenerator`/`PasswordGeneratorSettings` wrappers in v3?**
   Recommend **dropping them**. They have carried `[Obsolete]` since v2, they are the sole source of
   the 5 `CS0108` warnings, and v3 is a major version (the natural removal point). If you prefer
   maximum caution, the fallback is to keep them for one more major but add the `new` keyword to
   silence the warnings — but a clean removal is the better long-term call.
2. **Minimum target — `netstandard2.0` vs `net8.0;net10.0` only?**
   Recommend **multi-targeting `netstandard2.0;net8.0`** (optionally add `net10.0`). Dropping
   `netstandard2.0` would cut off .NET Framework / older consumers, which matters for this package's
   Umbraco-heavy audience. Multi-targeting lets the modern TFM use `RandomNumberGenerator.GetInt32`
   / `GetItems` (fixing the bias cleanly) while `netstandard2.0` keeps a manual rejection-sampling
   fallback.
3. **DI overload taking an `IConfiguration` section for one-line appSettings binding?**
   **Yes.** Provide both `AddPasswordGenerator(Action<PasswordOptions> configure)` and
   `AddPasswordGenerator(IConfiguration section)` so consumers can bind their policy from
   `appSettings.json` in a single line, consistent with the fluent > appSettings > default order.

---

## 5. Summary

- **9 of 10** original §5 bugs are **Confirmed still present**; **§5.5 (Guid shuffle) is Partially
  Fixed** — the addendum's premise that it was fully removed is inaccurate (`Password.cs:247-249`),
  though it is now redundant rather than a security hole.
- **§5.9** remains a **latent, non-reachable** footgun.
- **All §8 feature gaps confirmed absent**, except a custom alphabet is *hackily* achievable via
  `IncludeSpecial(string)` today.
- The adjusted plan keeps the Guid-shuffle removal as a **cleanup** task (not dropped), folds in all
  addendum additions (async, DI, benchmarks, presets, appSettings, `.Generate()`, migration guide),
  and answers the three open questions with recommendations.
