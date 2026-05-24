# PasswordGenerator — Full Package Documentation & v3 Review

> Purpose: a single, self-contained reference for the `PasswordGenerator` NuGet package as it
> stands today (v2.1.0). Written so it can be pasted into a Claude chat to plan v3. It covers
> what the package is, every public API, the internal implementation, confirmed bugs, design
> smells, build/test output, and a prioritised list of v3 candidate features.
>
> Date: 2026-05-24 · Current published version: 2.1.0 · Target framework: `netstandard2.0`
> Repo: https://github.com/prjseal/PasswordGenerator · Author: Paul Seal · License: MIT

---

## 1. What the package is

`PasswordGenerator` is a small .NET Standard 2.0 class library that generates random passwords
(and short numeric codes) according to configurable rules: which character classes to include
(lowercase, uppercase, numeric, special), the length, custom special-character sets, and a cap on
generation attempts. It is marketed as helping meet "OWASP requirements" and is widely used in
the Umbraco / .NET community.

- **Package id:** `PasswordGenerator`
- **Single dependency-free assembly** (no third-party runtime dependencies).
- **Distribution:** NuGet (`Install-Package PasswordGenerator`).
- **Randomness source:** `System.Security.Cryptography.RandomNumberGenerator` (CSPRNG).

### Solution layout

```
PasswordGenerator.sln
├── PasswordGenerator/                 (the library, packable)
│   ├── IPassword.cs                   public fluent interface
│   ├── Password.cs                    main implementation
│   ├── IPasswordSettings.cs           settings interface
│   ├── PasswordSettings.cs            settings implementation
│   ├── PasswordGenerator.cs           [Obsolete] back-compat wrapper class
│   ├── PasswordGeneratorSettings.cs   [Obsolete] back-compat settings subclass
│   ├── PasswordGenerator.csproj       SDK-style, PackageVersion 2.1.0, netstandard2.0
│   ├── PasswordGenerator.nuspec       STALE legacy nuspec (says 2.0.5) — see §7
│   └── readme.txt                     ASCII-art readme bundled in older package
├── PasswordGenerator.Tests/           (NUnit tests, netcoreapp2.2)
│   ├── BasicTests.cs                  16 tests against Password
│   └── ObsoleteTests.cs              8 tests against the obsolete PasswordGenerator
├── Readme.md                          GitHub readme (has stale docs — see §6)
├── appveyor.yml                       CI: AppVeyor, VS2017 image, publish_nuget: true
├── License.md, *.png
```

---

## 2. Public API (what callers can do today)

### 2.1 `IPassword` (the contract)

```csharp
public interface IPassword
{
    IPassword IncludeLowercase();
    IPassword IncludeUppercase();
    IPassword IncludeNumeric();
    IPassword IncludeSpecial();
    IPassword IncludeSpecial(string specialCharactersToInclude);
    IPassword LengthRequired(int passwordLength);
    string Next();
    IEnumerable<string> NextGroup(int numberOfPasswordsToGenerate);
}
```

### 2.2 `Password` constructors

| Constructor | Behaviour |
|---|---|
| `Password()` | All four classes on, length 16, maxAttempts 10000, `usingDefaults = true` |
| `Password(IPasswordSettings settings)` | Caller-supplied settings |
| `Password(int passwordLength)` | All four classes on, given length |
| `Password(bool lower, bool upper, bool numeric, bool special)` | Explicit classes, length 16, `usingDefaults = false` |
| `Password(bool…, int passwordLength)` | + length |
| `Password(bool…, int passwordLength, int maximumAttempts)` | + attempts cap |

Defaults: `DefaultPasswordLength = 16`, `DefaultMaxPasswordAttempts = 10000`, all `Include*` default `true`.

### 2.3 Fluent builders

`IncludeLowercase() / IncludeUppercase() / IncludeNumeric() / IncludeSpecial() / IncludeSpecial(string) / LengthRequired(int)` — each returns `this` for chaining. The first fluent
`Include*`/`Add*` call after a defaulted `Password()` flips `usingDefaults` off and **clears the
character set**, so `new Password().IncludeNumeric()` yields a numeric-only password (not
"defaults plus numeric").

### 2.4 Generation

- `string Next()` — returns one password, OR a human-readable error **string** on failure (see §5.1).
- `IEnumerable<string> NextGroup(int n)` — calls `Next()` n times; **does not de-duplicate**.

### 2.5 Settings (`IPasswordSettings` / `PasswordSettings`)

Character pools (constants in `PasswordSettings`):

```
Lowercase  = "abcdefghijklmnopqrstuvwxyz"
Uppercase  = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
Numeric    = "0123456789"
Special    = @"!#$%&*@\"          // DEFAULT special set (8 chars only)
MinLength  = 4    MaxLength = 256
```

`Add*` methods mutate and return the same instance; `AddSpecial(string)` overrides the special set.

### 2.6 Usage examples (from Readme)

```csharp
var pwd = new Password();                  var p = pwd.Next();   // 16 chars, all classes
var pwd = new Password(32);                                       // length 32
var pwd = new Password(true,true,false,false,21);                // letters only, len 21
var pwd = new Password().IncludeNumeric();                       // numeric only, len 16
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeSpecial();
var pwd = new Password(4).IncludeNumeric();                      // 4-digit OTP
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeNumeric().IncludeSpecial("[]{}^_=");
```

---

## 3. How generation actually works (internal flow)

`Next()` → validate requested length is in `[Min,Max]` → loop up to `MaximumAttempts`:
`GenerateRandomPassword(settings)` then `PasswordIsValid(settings, pwd)` → return first valid, else `"Try again"`.

`GenerateRandomPassword`:
1. Takes `settings.CharacterSet`, **shuffles** it via `OrderBy(Guid.NewGuid())`.
2. For each position, picks a char at `GetRandomNumberInRange(0, characterSetLength - 1)`.
3. Rejects a char that would make **3 identical in a row** (only checked from position > 2).

`GetRandomNumberInRange(min, max)`:
```csharp
var data = new byte[sizeof(int)];
_rng.GetBytes(data);
var randomNumber = BitConverter.ToInt32(data, 0);
return (int)Math.Floor((double)(min + Math.Abs(randomNumber % (max - min))));
```

`PasswordIsValid`: regex-checks at least one lowercase/uppercase/numeric is present (when required),
checks at least one special char from the configured set is present (when required & set non-empty),
and re-checks length. It does **not** verify the password contains *only* allowed characters.

---

## 4. Build & test results (verified in this environment)

Built with the .NET 8 SDK (8.0.421). The library targets `netstandard2.0`; tests target
`netcoreapp2.2`.

### 4.1 Library build — **succeeds, 5 warnings**

All five are `CS0108` member-hiding warnings on the obsolete `PasswordGenerator` class, because its
`IncludeLowercase/Uppercase/Numeric/Special` and `LengthRequired` methods hide the inherited
`Password` members without the `new` keyword:

```
PasswordGenerator.cs(36,34): warning CS0108: 'PasswordGenerator.IncludeLowercase()' hides inherited member 'Password.IncludeLowercase()'.
… (same for IncludeUppercase, IncludeNumeric, IncludeSpecial, LengthRequired)
```

### 4.2 Test project build — **succeeds, with vulnerability + obsolete warnings**

- `NU1903` (high) / `NU1902` (moderate): `Microsoft.NETCore.App` **2.2.0** has known
  high/moderate-severity vulnerabilities. The `netcoreapp2.2` target is **out of support**.
- Many `CS0618`: the `ObsoleteTests` intentionally exercise the obsolete `PasswordGenerator` class.

### 4.3 Tests — **24/24 pass**

`netcoreapp2.2` runtime is not installable (EOL), so tests were re-run on `net8.0` (NUnit 3.14).
Result: `Failed: 0, Passed: 24, Skipped: 0, Total: 24`. (16 in `BasicTests`, 8 in `ObsoleteTests`.)

> Note: the test names assert intent the code doesn't fully guarantee, e.g.
> `…10Passwords_ShouldReturn10DifferentPasswords` only asserts `Count() == 10`, never uniqueness.

---

## 5. Confirmed bugs & correctness issues

### 5.1 `Next()` returns error text *as if it were a password* (API design bug)
On invalid length it returns `"Password length invalid. Must be between 4 and 256 characters long"`;
if no valid password is produced within `MaximumAttempts` it returns `"Try again"`. A caller that
doesn't special-case these strings will happily store an error message as a user's password. There
is no exception, no `bool TryNext(out …)`, no `Result` type. **This is the single most important
correctness/safety issue.**

### 5.2 Off-by-one: the last character of the shuffled set is never selected (verified)
`GetRandomNumberInRange(0, characterSetLength - 1)` computes `Math.Abs(r % (max - min))` =
`r % (characterSetLength - 1)`, which yields `0 … characterSetLength-2`. The highest index is
**never** reachable. Empirically confirmed: for a 10-element range, index 9 is never produced.
Because the set is reshuffled per `GenerateRandomPassword` call, no single character is permanently
excluded across passwords, but **within each password the effective alphabet is one char smaller**
and the distribution is skewed.

### 5.3 Modulo bias (non-uniform distribution)
`r % n` over a full-range `Int32` is not uniform unless `n` divides 2^32. Character selection is
therefore slightly biased. For a security-focused generator that advertises a CSPRNG, the correct
approach is rejection sampling (e.g. `RandomNumberGenerator.GetInt32` on modern TFMs).

### 5.4 "Max 2 identical in a row" rule is mis-guarded
The check is `characterPosition > maximumIdenticalConsecutiveChars` (i.e. `> 2`), so it only starts
at position 3. The first three characters can be identical (e.g. a password starting `aaa…`). The
rule itself also reduces entropy intentionally — debatable whether it belongs in a password
generator at all.

### 5.5 Non-cryptographic, non-uniform shuffle
`Shuffle` uses `from item in items orderby Guid.NewGuid()`. `Guid.NewGuid()` is **not** a CSPRNG and
`OrderBy` over a random key is not a uniform (Fisher–Yates) shuffle. This undermines the
"cryptographically secure" positioning. (The per-character pick uses the CSPRNG, but the shuffle
layered on top adds weak, biased randomness.)

### 5.6 `_rng` is `static`, reassigned per instance, never disposed
`private static RandomNumberGenerator _rng;` is reassigned inside **every** constructor. Constructing
many `Password` objects repeatedly replaces the shared static field and leaks `IDisposable` RNG
instances (never disposed). It's also a surprising shared-state design for a class that otherwise
looks instance-scoped.

### 5.7 Dead code referencing the removed provider
`GetRngCryptoSeed(RNGCryptoServiceProvider rng)` is private, unused, and still references the legacy
`RNGCryptoServiceProvider` (the migration commit claimed to remove that usage). Should be deleted.

### 5.8 `IncludeSpecial` with an empty/whitespace custom set silently never validates
If `IncludeSpecial` is true but `SpecialCharacters` is null/whitespace, `specialIsValid` stays
`false`, so every attempt fails and `Next()` returns `"Try again"`. No guard / no error explaining why.

### 5.9 `Math.Abs(int.MinValue)` footgun (latent, NOT currently reachable)
Standalone `Math.Abs(int.MinValue)` throws `OverflowException` (verified). In this code it is **not**
reachable because `% (max - min)` is applied *before* `Math.Abs`, bounding the operand. Worth noting
so a v3 refactor doesn't accidentally expose it.

### 5.10 `NextGroup` does not guarantee uniqueness
Despite the test name implying "different passwords", duplicates are possible (astronomically
unlikely at length 16, but real for short numeric OTPs like a 4-digit code).

---

## 6. Documentation defects

- **Readme length claims are wrong.** `Readme.md` repeatedly says length "Must be between 8 and 128",
  but the code enforces **4 and 256** (`DefaultMinPasswordLength = 4`, `DefaultMaxPasswordLength = 256`).
- Code samples are fenced as ```javascript``` although they are C#.
- Readme logo points at branch `dev/v2`; compatibility image at `master`. Brittle.
- `IncludeSpecial(string)` exists on `Password`/`IPassword` but is **missing** from the obsolete
  `PasswordGenerator` wrapper — minor inconsistency.

---

## 7. Packaging / project hygiene

- **Version drift:** `PasswordGenerator.csproj` declares `2.1.0` (and `Version`, `AssemblyVersion`,
  `FileVersion` all 2.1.0). The legacy `PasswordGenerator.nuspec` still says **2.0.5** with 2019
  copyright and lists `RNGCryptoServiceProvider` in tags/notes. The nuspec appears stale/unused
  (SDK-style `.csproj` packs the package) and is misleading — decide whether to delete it.
- `<Copyright>Copyright 2022</Copyright>` in csproj vs `Copyright 2019` in nuspec.
- **No `README` packed into the NuGet package** via the modern `<PackageReadmeFile>` mechanism; only
  the old `readme.txt` ASCII-art file is referenced by the nuspec.
- **No SourceLink, no deterministic build, no symbol package (`snupkg`), no `<PackageIcon>`** (uses
  the deprecated `<PackageIconUrl>`).
- **CI is AppVeyor on the VS2017 image** (`appveyor.yml`) — very old; no GitHub Actions.
- Tests target EOL `netcoreapp2.2` and pull a vulnerable `Microsoft.NETCore.App 2.2.0`.

---

## 8. What the package does NOT do (feature gaps)

- No **passphrase / word-list** generation (e.g. diceware / xkcd-style).
- No **"exclude ambiguous characters"** option (e.g. `0/O`, `1/l/I`).
- No **per-class minimum counts** (e.g. "at least 2 digits and 1 special").
- No **"require at least one of each included class"** guarantee — it relies on retry+validate,
  which is probabilistic and can return `"Try again"`.
- No **entropy/strength estimate** for a generated password.
- No **pronounceable / memorable** password mode.
- No **`Span<char>`/allocation-efficient** API; everything is `string`/`char[]`/LINQ.
- No **async** API (not really needed, but absent).
- No **dependency-injection helpers** (`AddPasswordGenerator()` / `IServiceCollection` extension).
- No **`TryNext`/`Result`** pattern — failures are encoded as magic strings.
- No **custom character pools** beyond special chars (can't, say, supply a full custom alphabet).
- No **uniqueness guarantee** in `NextGroup`.
- No **`net6/net8` target** — `netstandard2.0` only (works everywhere, but misses
  `RandomNumberGenerator.GetInt32`, `GetItems`, etc.).
- No **nullable reference type** annotations.

---

## 9. Suggested v3 direction (prioritised, for discussion)

**Tier 1 — correctness & security (do these regardless):**
1. Replace the error-string returns with proper failure handling: throw `ArgumentException` for
   invalid config and add `bool TryNext(out string password)` and/or a `PasswordResult` type. (§5.1)
2. Fix character selection: drop the Guid shuffle + biased modulo; use unbiased rejection sampling
   (`RandomNumberGenerator.GetInt32` on modern TFMs, manual rejection on `netstandard2.0`). Fixes
   §5.2, §5.3, §5.5 at once.
3. Guarantee included classes deterministically (seed one of each required class, then fill & shuffle)
   instead of generate-and-retry; removes `"Try again"` and `MaximumAttempts` entirely.
4. Remove dead code (`GetRngCryptoSeed`) and fix the `static`/undisposed `_rng` design. (§5.6, §5.7)

**Tier 2 — API & packaging modernisation:**
5. Multi-target `netstandard2.0;net8.0` (and maybe `net6.0`); add nullable annotations.
6. Add DI extension `services.AddPasswordGenerator()`.
7. Fix versioning/packaging: delete or regenerate the stale nuspec, add `<PackageReadmeFile>`,
   SourceLink, deterministic builds, `snupkg`, `<PackageIcon>`. Move CI to GitHub Actions.
8. Update tests to a supported TFM (`net8.0`) and NUnit 4, add uniqueness/entropy/edge-case tests.

**Tier 3 — new features:**
9. "Exclude ambiguous characters" option and per-class minimum counts.
10. Passphrase/diceware mode with a bundled word list.
11. Entropy / strength estimate on the result.
12. `NextGroup` uniqueness option.

**Breaking-change note:** items 1 and 3 change the failure contract and remove `MaximumAttempts`
semantics — appropriate for a major (v3) bump. Decide whether to keep the obsolete
`PasswordGenerator`/`PasswordGeneratorSettings` classes or finally drop them in v3.

---

## 10. Quick reference — files & key line anchors

- `Password.cs:114` `Next()` (error-string returns at lines 119, 131)
- `Password.cs:157` `GenerateRandomPassword` (shuffle at 163; 3-in-a-row guard at 172–177)
- `Password.cs:183` `GetRandomNumberInRange` (off-by-one + modulo bias)
- `Password.cs:195` `GetRngCryptoSeed` (dead code, references `RNGCryptoServiceProvider`)
- `Password.cs:208` `PasswordIsValid` (special-char empty-set edge case at 221–229)
- `Password.cs:247` `Shuffle` (Guid-based, non-uniform)
- `PasswordSettings.cs:10-15` character pools + min/max length (4/256)
- `PasswordSettings.cs:102` `StopUsingDefaults` (clears set on first fluent call)
- `PasswordGenerator.cs:5` `[Obsolete]` wrapper (source of the 5 CS0108 warnings)
- `PasswordGenerator.csproj:5,21` version 2.1.0 vs `PasswordGenerator.nuspec:5` version 2.0.5
