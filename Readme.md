# Password Generator

![Password Logo](https://github.com/prjseal/PasswordGenerator/blob/dev/v2/passwordgeneratorlogo.png "Password Logo")

A cross-platform .NET library that generates cryptographically secure random passwords, passphrases,
OTPs, API keys and readable identifiers. Configure it with a fluent API, ready-made presets
(OWASP/NIST) or dependency injection — with async support and entropy estimation.

## NuGet

Install via NuGet: ``` Install-Package PasswordGenerator ```

[![Nuget Downloads](https://img.shields.io/nuget/dt/PasswordGenerator.svg)](https://www.nuget.org/packages/PasswordGenerator)

[Or click here to go to the package landing page](https://www.nuget.org/packages/PasswordGenerator)

It targets `net8.0` and `net10.0`, so it requires .NET 8 or later. If you need to run on .NET
Framework or other older runtimes, use the 2.x line (which targets `netstandard2.0`).

> **Upgrading from 2.x?** See the [v2 → v3 migration guide](docs/migration-v2-to-v3.md).
> The v2 API still works; the one behavioural change is that invalid settings now **throw** (or use
> `TryNext`) instead of returning an error string as the "password".

## Basic usage

```csharp
// By default, all character types are available and the length is 16.
// Returns a random password with the default settings.
var pwd = new Password();
var password = pwd.Next();
```

```csharp
// Set the length. Must be between 4 and 256.
// Returns a password that is 32 characters long.
var pwd = new Password(32);
var password = pwd.Next();
```

```csharp
// Choose which character types to include.
// Returns a 21-character password of lowercase and uppercase letters only.
var pwd = new Password(includeLowercase: true, includeUppercase: true, includeNumeric: false, includeSpecial: false, passwordLength: 21);
var password = pwd.Next();
```

## Fluent usage

```csharp
// Build up your requirements by chaining, e.g. .IncludeNumeric()
// Returns a numbers-only password with the default length of 16.
var pwd = new Password().IncludeNumeric();
var password = pwd.Next();
```

```csharp
// Combine lower, upper and special characters the same way.
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeSpecial();
var password = pwd.Next();
```

```csharp
// As above, but with a length of 128.
var pwd = new Password(128).IncludeLowercase().IncludeUppercase().IncludeSpecial();
var password = pwd.Next();
```

```csharp
// As above, but passing the length via LengthRequired().
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeSpecial().LengthRequired(128);
var password = pwd.Next();
```

```csharp
// Specify your own special characters.
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeNumeric().IncludeSpecial("[]{}^_=");
var password = pwd.Next();
```

## Presets

Ready-made starting points; later fluent calls still override them. See the
[standards mapping](docs/migration-v2-to-v3.md#6-standards-mapping-for-the-presets) for the
OWASP/NIST rationale.

```csharp
string strong  = Password.ForOwasp().Next();            // full printable-ASCII pool, length 16
string nist    = Password.ForNist().Next();             // NIST-aligned, length 12
string otp     = Password.ForOtp(6).Next();             // 6-digit one-time code
string apiKey  = Password.ForApiKey(32).Next();         // URL-safe token
string envName = Password.ForEnvironmentName(12).Next();// readable id, no look-alike characters
string phrase  = Password.ForPassphrase(4).Next();      // e.g. "maple-river-quartz-bloom-42"
string strong  = Password.ForPassphraseWithEntropy(80).Next(); // word count derived to clear 80 bits
string memorable = Password.ForMemorable().Next();      // capitalized, ~80+ bits, e.g. "Maple-River-Quartz-Bloom-Glade-Vivid-42"
```

`ForPassphraseWithEntropy(targetBits)` derives the word count needed to reach the target and
enforces it as a floor. You can also pass `minimumEntropyBits` to `ForPassphrase(...)` to reject
configurations that are too weak.

For sites that require a digit and a symbol, pass `includeSymbol: true`. A random symbol is attached
to one randomly chosen word (e.g. `maple-river#-quartz-bloom-42`), so the phrase passes composition
rules while staying memorable.

## Quality controls

```csharp
// Remove look-alike characters (I l 1 O 0 o)
var readable = new Password(20).ExcludeAmbiguous().Next();

// Guarantee at least N characters from a class
var pwd = new Password(16).RequireAtLeast(CharacterClass.Numeric, 2).Next();

// Use a custom pool, or every printable ASCII character
var custom = new Password().WithCharacters("ABCDEF0123456789").LengthRequired(24).Next();
var ascii  = new Password().WithAllAscii().LengthRequired(40).Next();

// Estimate strength in bits
double bits = new Password(20).EstimateEntropyBits();
```

## Error handling

```csharp
// Next() throws ArgumentException when the settings can't produce a valid password.
var password = new Password(16).Next();

// TryNext() never throws; it returns false on invalid settings.
if (new Password(16).TryNext(out var result))
    Console.WriteLine(result);
```

## Async and batches

```csharp
string password           = await pwd.NextAsync(cancellationToken);
IReadOnlyList<string> ten = pwd.Generate(10);
IReadOnlyList<string> ten2 = await pwd.GenerateAsync(10, cancellationToken);
```

## Dependency injection

```csharp
// Register once (optionally bind from appSettings.json)
services.AddPasswordGenerator(o =>
{
    o.Length = 20;
    o.IncludeSpecial = true;
    o.ExcludeAmbiguous = true;
});

// Inject IPasswordGenerator wherever you need it
public class SignupService(IPasswordGenerator generator)
{
    public string NewTempPassword() => generator.Next();
}
```

To register a passphrase generator instead, set the `Passphrase` options (or bind a `Passphrase`
section from configuration):

```csharp
services.AddPasswordGenerator(o =>
    o.Passphrase = new PassphraseOptions { WordCount = 6, Capitalize = true });
```

## Documentation

- [v2 → v3 migration guide](docs/migration-v2-to-v3.md)
- [Changelog](CHANGELOG.md)
- [Design & architecture docs](docs/README.md)

## License & attribution

PasswordGenerator is licensed under the [MIT License](License.md).

Passphrases are generated from the **EFF Large Wordlist** (7,776 words) by the
[Electronic Frontier Foundation](https://www.eff.org/dice), used under the
[Creative Commons Attribution 3.0 US](https://creativecommons.org/licenses/by/3.0/us/)
license. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for details.
