![PasswordGenerator Logo](https://github.com/prjseal/PasswordGenerator/blob/master/pg_logo.png "PasswordGenerator Logo")

A library which generates random passwords with different settings to meet the OWASP requirements

## NuGet

Install via NuGet: ``` Install-Package PasswordGenerator ```

[![Nuget Downloads](https://img.shields.io/nuget/dt/PasswordGenerator.svg)](https://www.nuget.org/packages/PasswordGenerator)

[Or click here to go to the package landing page](https://www.nuget.org/packages/PasswordGenerator)

## Basic usage

See examples below or try them out now in your browser using [Dotnetfiddle](https://dotnetfiddle.net/Q0hMlU)

```javascript
// By default, all characters available for use and a length of 16
// Will return a random password with the default settings 
PasswordGenerator pwdGen = new PasswordGenerator();
string password = pwdGen.Next();
```

```javascript
// Same as above but you can set the length. Must be between 8 and 128
// Will return a password which is 32 characters long
PasswordGenerator pwdGen = new PasswordGenerator(32);
string password = pwdGen.Next();
```

```javascript
// Same as above but you can set the length. Must be between 8 and 128
// Will return a password which only contains lowercase and uppercase characters and is 21 characters long.
PasswordGenerator pwdGen = new PasswordGenerator(includeLowercase: true, includeUppercase: true, includeNumeric: false, includeSpecial: false, passwordLength: 21);
string password = pwdGen.Next();
```

## Fluent usage

```javascript
// You can build up your reqirements by adding things to the end, like .IncludeNumeric()
// This will return a password which is just numbers and has a default length of 16
PasswordGenerator pwdGen = new PasswordGenerator().IncludeNumeric();
string password = pwdGen.Next();
```

```javascript
// As above, here is how to get lower, upper and special characters using this approach
PasswordGenerator pwdGen = new PasswordGenerator().IncludeLowercase().IncludeUppercase().IncludeSpecial();
string password = pwdGen.Next();
```

```javascript
// This is the same as the above, but with a length of 128
PasswordGenerator pwdGen = new PasswordGenerator(128).IncludeLowercase().IncludeUppercase().IncludeSpecial();
string password = pwdGen.Next();
```

```javascript
// This is the same as the above, but with passes the length in using the method LengthRequired()
PasswordGenerator pwdGen = new PasswordGenerator().IncludeLowercase().IncludeUppercase().IncludeSpecial().LengthRequired(128);
string password = pwdGen.Next();
```

### Compatible with .NET Core

You can use this library with .NET Core. [Read this post by Jamie Taylor](https://dotnetcore.gaprogman.com/2017/06/01/net-core-and-net-framework-working-together-or-the-magic-of-net-standard/) who shows you how to do it.
