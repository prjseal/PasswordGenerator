# Password Generator Binary Cmdlet for PowerShell
A fork of the popular [PasswordGenerator](https://www.nuget.org/packages/PasswordGenerator) nuget package used for dotnet projects. The goal of this repository is to port the nuget package to a PowerShell module with similar or exact functionallity, allowing PowerShell module authors and admins to take advantage of a fast and customizable Password Generator.

## Usage

### Examples:


```powershell
# By default, all characters available for use and a length of 16

# Will return a random password with the default settings

New-Password
```

```powershell
# Same as above but you can set the length. Must be between 4 and 128

# Will return a password which is 32 characters long

New-Password -Length 32
```

```powershell
# Same as above but you can set the length. Must be between 4 and 128

# Will return a password which only contains lowercase and uppercase characters and is 21 characters long.

New-Password -IncludeLowercase -IncludeUppercase -Length 21
```


```powershell
# You can build up your reqirements by adding parameters, like -IncludeNumeric

# This will return a password which is just numbers and has a default length of 16

New-Password -IncludeNumeric
```

```powershell
# As above, here is how to get lower, upper and special characters using this approach

New-Password -IncludeLowercase -IncludeUppercase -IncludeSpecial
```

```powershell
# This is the same as the above, but with a length of 128

New-Password -IncludeLowercase -IncludeUppercase -IncludeSpecial -Length 128
```

```powershell
# One Time Passwords

# If you want to return a 4 digit number you can use this:

New-Password -IncludeNumeric -Length 4
```

### Example 8
```powershell
# Returns a 16 length complex password as a System.SecureString object

PS C:\>  New-Password | ConvertTo-SecureString -AsPlainText -Force
```


## Planned Features, up for grabs :)

```powershell
# Specify your own special characters

# You can specify your own special characters

New-Password -IncludeLowercase -IncludeUppercase -IncludeNumeric -IncludeSpecial "!%¤/:)"
```

## Compatibility

Since the nuget package is based on netstandard2.0, the module currently works on:
- Newer versions of PowerShell (6+) 
- Older versions of PowerShell (Tested on 5.1)
- Linux, Mac versions of PowerShell (6+)
