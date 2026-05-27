# Migrating from v2.x to v3.0

v3 is a major release, but the **v2 public surface still compiles and runs** — constructors,
`IncludeLowercase()/…`, `LengthRequired()`, `Next()` and `NextGroup()` are all intact. Most projects
upgrade by just bumping the package. The sections below cover the one behavioural change you must know
about, plus the new capabilities you can adopt at your own pace.

> **Length range:** valid password lengths are **4–256** characters (the old "8–128" Readme claim was
> never the actual limit).

> **Runtime requirement:** v3 targets `net8.0` and `net10.0` and **drops `netstandard2.0`**. You need
> .NET 8 or later. Projects on .NET Framework or other older runtimes should stay on the 2.x line.

---

## 1. The one breaking change: error strings → exceptions / `TryNext`

In v2, invalid settings caused `Next()` to **return an error message as if it were a password** (e.g.
`"Try again"` or a "Password length invalid…" string). In v3 `Next()` **throws**, and a non-throwing
`TryNext` is provided.

```csharp
// v2 — the "password" might actually be an error string
var pwd = new Password(passwordLength: 2); // below the minimum
var password = pwd.Next();                 // returns "Password length invalid. Must be between …"

// v3 — fail loudly…
var password = new Password(2).Next();     // throws ArgumentException

// …or fail softly
if (new Password(2).TryNext(out var password))
    Use(password);
else
    // settings were invalid; password is null
```

**Action:** if you relied on the returned string to detect failure, switch to `TryNext` or wrap
`Next()` in a `try/catch (ArgumentException)`.

---

## 2. Direct construction → dependency injection (optional)

Direct `new Password(...)` still works. If you use `Microsoft.Extensions.DependencyInjection`, you can
now register the generator instead.

```csharp
// v2 / still valid in v3
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeNumeric();
var password = pwd.Next();

// v3 — register once…
services.AddPasswordGenerator(o =>
{
    o.Length = 20;
    o.IncludeSpecial = true;
});

// …then inject IPasswordGenerator anywhere
public class SignupService(IPasswordGenerator generator)
{
    public string NewTempPassword() => generator.Next();
}
```

Bind from `appSettings.json`, with **code overrides taking precedence over configuration**:

```csharp
// resolution order: code-configure  >  appSettings  >  default
services.AddPasswordGenerator(configuration.GetSection("PasswordGenerator"), o => o.Length = 24);
```

```json
{
  "PasswordGenerator": {
    "Length": 16,
    "IncludeSpecial": true,
    "ExcludeAmbiguous": true,
    "DefaultBatchCount": 1
  }
}
```

---

## 3. Synchronous → async (optional)

Sync methods are unchanged. v3 adds `async` overloads for call sites that want them (they complete
synchronously but honour cancellation):

```csharp
var password   = await generator.NextAsync(cancellationToken);
var passwords  = await generator.GenerateAsync(count: 10, cancellationToken);
```

---

## 4. Batch generation

```csharp
// v2 — still works
IEnumerable<string> many = new Password().NextGroup(10);

// v3 — explicit count…
IReadOnlyList<string> ten = generator.Generate(10);

// …or the parameterless overload, driven by PasswordOptions.DefaultBatchCount
IReadOnlyList<string> defaultBatch = generator.Generate();
```

> Batch results are **not de-duplicated** — collisions are astronomically unlikely at realistic
> lengths, and forcing uniqueness would bias the distribution.

---

## 5. New capabilities to adopt

| Need | v3 API |
|---|---|
| Custom character pool | `new Password().WithCharacters("ABC123")` |
| Every printable ASCII char | `new Password().WithAllAscii()` |
| Drop look-alikes (`I l 1 O 0 o`) | `…ExcludeAmbiguous()` |
| Guarantee N of a class | `…RequireAtLeast(CharacterClass.Numeric, 2)` |
| Strength estimate (bits) | `new Password(20).EstimateEntropyBits()` |
| Presets | `Password.ForOwasp()`, `ForNist()`, `ForOtp()`, `ForApiKey()`, `ForEnvironmentName()`, `ForPassphrase()` |

---

## 6. Standards mapping for the presets

The presets are convenience starting points; later fluent calls still override them.

| Preset | Intent | Reference |
|---|---|---|
| `Password.ForOwasp(length = 16)` | Long secret over the full printable-ASCII pool, no forced composition rules. | [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#implement-proper-password-strength-controls) |
| `Password.ForNist(length = 12)` | Length-first, no composition-rule penalties, broad character support. | [NIST SP 800-63B §5.1.1](https://pages.nist.gov/800-63-3/sp800-63b.html#memsecret) |
| `Password.ForPassphrase(words = 4)` | Diceware-style multi-word secret (memorable, high entropy per length). | [NIST SP 800-63B (memorized secrets)](https://pages.nist.gov/800-63-3/sp800-63b.html#memsecret) |

> OWASP and NIST both **discourage composition rules** (forcing symbol/number mixes) in favour of
> length and screening, which is why `ForOwasp`/`ForNist` use the full pool without per-class minimums.
> When a downstream system *requires* composition, layer it on explicitly with `RequireAtLeast`.

---

## 7. Beyond passwords — what else v3 generates

PasswordGenerator is a general cryptographically-secure secret generator:

```csharp
string otp     = Password.ForOtp(6).Next();              // "418207"
string apiKey  = Password.ForApiKey(32).Next();          // URL-safe token
string envName = Password.ForEnvironmentName(12).Next(); // readable, no look-alikes
string phrase  = Password.ForPassphrase(4).Next();       // "maple-river-quartz-bloom-42"
```

---

**Docs:** [← Configuration & DI](configuration-and-di.md) · [Index](README.md) · Next → [Local NuGet test report](v3-local-nuget-test.md)
