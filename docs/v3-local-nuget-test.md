# PasswordGenerator v3.0.0 — local NuGet package test report

This document records an end-to-end test of the **PasswordGenerator 3.0.0** package
(built from the `dev/v3` branch) as a real consumer would experience it: the package was
packed locally, served from a local NuGet feed, installed into a fresh console app, and
exercised across every documented scenario. For each scenario you get the exact code that
was added, the real output it produced, and a note on whether that output was **expected**.

## How the package was built and consumed

These are the exact steps a user would follow to reproduce this report.

### 1. Pack the library from `dev/v3`

```bash
git checkout dev/v3
git pull origin dev/v3
dotnet pack PasswordGenerator/PasswordGenerator.csproj -c Release -o /tmp/localnuget
```

Output (trimmed):

```
PasswordGenerator -> .../bin/Release/netstandard2.0/PasswordGenerator.dll
PasswordGenerator -> .../bin/Release/net8.0/PasswordGenerator.dll
Successfully created package '/tmp/localnuget/PasswordGenerator.3.0.0.nupkg'.
Successfully created package '/tmp/localnuget/PasswordGenerator.3.0.0.snupkg'.
```

The package multi-targets `netstandard2.0` and `net8.0`, and a `.snupkg` symbol package is
produced alongside it. **Expected** — this matches the packaging notes in `CHANGELOG.md`.

> The only build warnings were `SourceLink` notices that the source-control information is
> empty. That is expected when packing from a plain working tree (no CI commit metadata) and
> does not affect the produced assemblies.

### 2. Create a test project and register the local feed

```bash
dotnet new console -n PgTestApp -o .
dotnet nuget add source /tmp/localnuget --name LocalPgSource
```

`dotnet nuget list source` then shows the local feed registered alongside nuget.org:

```
1.  nuget.org      [Enabled]   https://api.nuget.org/v3/index.json
2.  LocalPgSource  [Enabled]   /tmp/localnuget
```

### 3. Install the package from the local feed

```bash
dotnet add package PasswordGenerator --version 3.0.0 --source /tmp/localnuget
```

```
info : Installed PasswordGenerator 3.0.0 from /tmp/localnuget ...
info : Package 'PasswordGenerator' is compatible with all the specified frameworks ...
```

**Expected** — the package resolves from the local source and is compatible with the
`net8.0` test project.

For the dependency-injection / `appsettings.json` scenarios two more packages were added:

```bash
dotnet add package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 8.0.0
```

### Resulting `PgTestApp.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="PasswordGenerator" Version="3.0.0" />
  </ItemGroup>
</Project>
```

### `appsettings.json` (used by scenario 12)

```json
{
  "PasswordGenerator": {
    "Length": 24,
    "IncludeLowercase": true,
    "IncludeUppercase": true,
    "IncludeNumeric": true,
    "IncludeSpecial": false,
    "ExcludeAmbiguous": true,
    "DefaultBatchCount": 3
  }
}
```

---

## Scenarios

> Passwords are random, so the exact characters differ on every run. The notes focus on the
> properties that should hold (length, character classes, behaviour), not the literal value.

### 1. Basic default

```csharp
var pwd = new Password();
var password = pwd.Next();
```

Output:

```
value='RHX%0Bzgz@f8R4rI' length=16
```

**Expected.** Default length is 16 and all four character classes are available.

### 2. Set the length

```csharp
var password = new Password(32).Next();
```

Output:

```
value='Ck4JOCY!8uNCQR0o4qn7jKx\Q%cAM0%3' length=32
```

**Expected.** Length honoured exactly.

### 3. Choose which character types to include

```csharp
var password = new Password(
    includeLowercase: true, includeUppercase: true,
    includeNumeric: false, includeSpecial: false,
    passwordLength: 21).Next();
```

Output:

```
value='KDTwPmSwycfVDTKuEwRXQ' length=21
```

**Expected.** Letters only — no digits or specials — at the requested length of 21.

### 4. Fluent: numeric only

```csharp
var password = new Password().IncludeNumeric().Next();
```

Output:

```
value='1542580664200162' length=16
```

**Expected.** A digits-only password at the default length of 16. The fluent
`IncludeNumeric()` resets the pool to the single requested class.

### 5. Fluent: lower + upper + special, length 128

```csharp
var password = new Password(128)
    .IncludeLowercase().IncludeUppercase().IncludeSpecial().Next();
```

Output:

```
value='Xq#$PLzbuSFdBSkvwMbKoPYxlE@BQJmp\um%g&\n*qCQz...' length=128
```

**Expected.** Length 128 produced. (The `\n` in the value is a literal backslash followed by
`n` — `\` is part of the special-character set — not a newline.)

### 6. Custom special characters

```csharp
var password = new Password()
    .IncludeLowercase().IncludeUppercase().IncludeNumeric()
    .IncludeSpecial("[]{}^_=").Next();
```

Output:

```
value='1hAJT5uB6p]swPrI' length=16
```

**Expected.** Any special character present comes only from the supplied set (`]` here).

### 7. Presets

```csharp
Console.WriteLine(Password.ForOwasp().Next());
Console.WriteLine(Password.ForNist().Next());
Console.WriteLine(Password.ForOtp(6).Next());
Console.WriteLine(Password.ForApiKey(32).Next());
Console.WriteLine(Password.ForEnvironmentName(12).Next());
Console.WriteLine(Password.ForPassphrase(4).Next());
```

Output:

```
ForOwasp()           = 'ma4n'q^g"GH=X8$t'      (length 16, full printable ASCII)
ForNist()            = 'EPsw/Ib9S!'|'           (length 12, full ASCII)
ForOtp(6)            = '481386'                  (6 numeric digits)
ForApiKey(32)        = 'oCj6ppytpoqdzFvivPaIOiYbBjsChu4g'  (URL-safe, length 32)
ForEnvironmentName() = 'b9w8wxvbt35f'            (lowercase + digits, no look-alikes)
ForPassphrase(4)     = 'umber-acid-shine-salad-16'  (4 words + number, '-' separated)
```

**Expected.** Each preset matches its documented shape: OWASP = full ASCII/length 16,
NIST = full ASCII/length 12, OTP = numeric code, API key = URL-safe token, environment name =
readable id with ambiguous characters removed, passphrase = diceware words plus a number.

### 8. Quality controls

```csharp
var readable = new Password(20).ExcludeAmbiguous().Next();
var req      = new Password(16).RequireAtLeast(CharacterClass.Numeric, 2).Next();
var custom   = new Password().WithCharacters("ABCDEF0123456789").LengthRequired(24).Next();
var ascii    = new Password().WithAllAscii().LengthRequired(40).Next();
double bits  = new Password(20).EstimateEntropyBits();
```

Output:

```
ExcludeAmbiguous(20)       = '%C\CAiS63wsz**nrLx6!'
RequireAtLeast(Numeric,2)  = '624eWC#w%Sb8U8Zh'
WithCharacters(hex) len 24 = '4D1B017D148873963C0475A2'
WithAllAscii() len 40      = ']Ef;:(:d*{jJZh'vVa.!Vj.Gb!6Bys9to>m>q}Ja'
EstimateEntropyBits(20)    = 122.58566033889934
```

**Expected.**
- `ExcludeAmbiguous` output contains none of `I l 1 O 0 o`.
- `RequireAtLeast(Numeric, 2)` contains at least two digits (`6 2 4 8 8`).
- `WithCharacters` restricts output to the supplied hex alphabet only, at length 24.
- `WithAllAscii` uses the full printable-ASCII pool at length 40.
- `EstimateEntropyBits(20)` returns a positive bit estimate (~122.6 bits) consistent with a
  20-character password over the default multi-class pool.

### 9. Error handling

```csharp
// Valid settings via TryNext
if (new Password(16).TryNext(out var result))
    Console.WriteLine(result);

// Special required but empty special set
new Password().IncludeSpecial("").Next();   // expected to throw

// Length below the minimum
new Password(2).Next();                       // expected to throw

// TryNext never throws
var ok = new Password(2).TryNext(out var r2);
```

Output:

```
TryNext valid -> true, '4WsV&4z\$&Ksix\F'
Next() with empty special -> threw ArgumentException: Special characters are required but no special characters have been provided.
Next() length 2 -> threw ArgumentException: Password length invalid. Must be between 4 and 256 characters long
TryNext length 2 -> False, result is null: True
```

**Expected.** This is the headline v3 breaking change: invalid settings now **throw**
`ArgumentException` from `Next()` (rather than returning an error string as the "password"),
while `TryNext` returns `false` and a `null` password instead of throwing.

### 10. Async and batches

```csharp
var pwd = new Password();
string asyncPwd = await pwd.NextAsync(CancellationToken.None);
IReadOnlyList<string> five = pwd.Generate(5);
IReadOnlyList<string> three = await pwd.GenerateAsync(3, CancellationToken.None);
```

Output:

```
NextAsync()    = 'j%FCN%i4Q&#bvUgR'
Generate(5)    = 5 items
   y3nbjyLgRKkrJ$T#
   3#&G$#vXU$3*mq!9
   CeN0FRM#RP9y@ryN
   WlZ%IxX@kIrem8oc
   Ysi@i*#qjA9SL0g8
GenerateAsync(3) = 3 items
```

**Expected.** `NextAsync` returns a single password; `Generate(n)` / `GenerateAsync(n)`
return exactly `n` distinct passwords.

### 11. Dependency injection (configured in code)

```csharp
var services = new ServiceCollection();
services.AddPasswordGenerator(o =>
{
    o.Length = 20;
    o.IncludeSpecial = true;
    o.ExcludeAmbiguous = true;
});
using var sp = services.BuildServiceProvider();
var gen = sp.GetRequiredService<IPasswordGenerator>();
var password = gen.Next();
```

Output:

```
DI(code) Next() = 'b#3$%zQ6j4xGtYBjYt&K' length=20
```

**Expected.** `AddPasswordGenerator` registers `IPasswordGenerator`; the resolved generator
honours the code-configured options (length 20, specials on, ambiguous removed).

### 12. Dependency injection (bound from `appsettings.json`)

```csharp
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddPasswordGenerator(config.GetSection("PasswordGenerator"));
using var sp = services.BuildServiceProvider();
var gen = sp.GetRequiredService<IPasswordGenerator>();

var password = gen.Next();
var batch = gen.Generate();   // uses DefaultBatchCount from config
```

Output:

```
DI(config) Next()   = '54LWWrw8F4fMHfQDSjPWjawP' length=24 (config Length=24, ExcludeAmbiguous, no special)
DI(config) Generate() default batch count = 3 (config DefaultBatchCount=3)
```

**Expected.** Options bind from the `PasswordGenerator` configuration section: length 24, no
special characters, ambiguous characters removed, and `Generate()` (parameterless) returns 3
passwords matching `DefaultBatchCount`.

---

## Summary

| # | Scenario | Result | Expected? |
|---|----------|--------|-----------|
| – | `dotnet pack` (netstandard2.0 + net8.0, .snupkg) | Built | Yes |
| – | Register local feed + install package | Installed, framework-compatible | Yes |
| 1 | Basic default | length 16, all classes | Yes |
| 2 | Explicit length 32 | length 32 | Yes |
| 3 | Letters only, length 21 | letters only, length 21 | Yes |
| 4 | Fluent numeric only | digits only, length 16 | Yes |
| 5 | Fluent length 128 | length 128 | Yes |
| 6 | Custom special chars | specials from supplied set only | Yes |
| 7 | Presets (OWASP/NIST/OTP/API/Env/Passphrase) | each matches documented shape | Yes |
| 8 | Quality controls + entropy | ambiguous removed, minimums met, custom pool, entropy estimate | Yes |
| 9 | Error handling (throw / TryNext) | invalid settings throw; TryNext returns false/null | Yes |
| 10 | Async + batches | correct counts | Yes |
| 11 | DI (code configured) | options honoured | Yes |
| 12 | DI (appsettings.json) | options + DefaultBatchCount bound | Yes |

**Every scenario behaved as expected.** The package packs cleanly, installs from a local
NuGet feed, and the public API — fluent builder, presets, quality controls, error handling,
async, batch generation, and both dependency-injection registration paths — all behave as
documented in the Readme and CHANGELOG. The only non-fatal note during the whole run was the
empty-SourceLink build warning, which is expected when packing outside CI.
