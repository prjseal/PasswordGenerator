# Password Generator

![Password Logo](https://github.com/prjseal/PasswordGenerator/blob/dev/v2/passwordgeneratorlogo.png "Password Logo")

A .NET Standard library which generates random passwords with different settings to meet the OWASP requirements

## NuGet

Install via NuGet: ``` Install-Package PasswordGenerator ```

[![Nuget Downloads](https://img.shields.io/nuget/dt/PasswordGenerator.svg)](https://www.nuget.org/packages/PasswordGenerator)

[Or click here to go to the package landing page](https://www.nuget.org/packages/PasswordGenerator)

It is Compatible with .NET Core, .NET Framework and more. See the below chart:

![Compatibility Chart](https://github.com/prjseal/PasswordGenerator/blob/master/compatibility.png "Compatibility Chart")


## Basic usage

See examples below or try them out now in your browser using [Dotnetfiddle](https://dotnetfiddle.net/Q0hMlU)

```javascript
// By default, all characters available for use and a length of 16
// Will return a random password with the default settings 
var pwd = new Password();
var password = pwd.Next();
```

```javascript
// Same as above but you can set the length. Must be between 8 and 128
// Will return a password which is 32 characters long
var pwd = new Password(32);
var password = pwd.Next();
```

```javascript
// Same as above but you can set the length. Must be between 8 and 128
// Will return a password which only contains lowercase and uppercase characters and is 21 characters long.
var pwd = new Password(includeLowercase: true, includeUppercase: true, includeNumeric: false, includeSpecial: false, passwordLength: 21);
var password = pwd.Next();
```

## Fluent usage

```javascript
// You can build up your reqirements by adding things to the end, like .IncludeNumeric()
// This will return a password which is just numbers and has a default length of 16
var pwd = new Password().IncludeNumeric();
var password = pwd.Next();
```

```javascript
// As above, here is how to get lower, upper and special characters using this approach
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeSpecial();
var password = pwd.Next();
```

```javascript
// This is the same as the above, but with a length of 128
var pwd = new Password(128).IncludeLowercase().IncludeUppercase().IncludeSpecial();
var password = pwd.Next();
```

```javascript
// This is the same as the above, but with passes the length in using the method LengthRequired()
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeSpecial().LengthRequired(128);
var password = pwd.Next();
```

```javascript
// One Time Passwords
// If you want to return a 4 digit number you can use this:
var pwd = new Password(4).IncludeNumeric();
var password = pwd.Next();
```

```javascript
// Specify your own special characters
// You can now specify your own special characters
var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeNumeric().IncludeSpecial("[]{}^_=");
var password = pwd.Next();
```
