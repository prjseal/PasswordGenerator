# Password Generator Binary Cmdlet for PowerShell

This repo is a fork of the popular PasswordGenerator nuget package used for dotnet projects. The goal of this repository is to port the nuget package to a powershell module with similar functionallity, allowing PowerShell developers to take advantage of a fast and customizable Password Generator. 

## Basic usage

### Examples:



```powershell
# By default, all characters available for use and a length of 16

# Will return a random password with the default settings

Get-Password
```

```powershell
# Same as above but you can set the length. Must be between 4 and 128

# Will return a password which is 32 characters long

Get-Password -Length 32
```

```powershell
# Same as above but you can set the length. Must be between 4 and 128

# Will return a password which only contains lowercase and uppercase characters and is 21 characters long.

Get-Password -IncludeLowercase -IncludeUppercase -Length 21
```

## Advanced Usage

```powershell
# You can build up your reqirements by adding parameters, like -IncludeNumeric

# This will return a password which is just numbers and has a default length of 16
Get-Password -IncludeNumeric
```

```powershell
# As above, here is how to get lower, upper and special characters using this approach

Get-Password -IncludeLowercase -IncludeUppercase -IncludeSpecial
```

```powershell
# This is the same as the above, but with a length of 128

Get-Password -IncludeLowercase -IncludeUppercase -IncludeSpecial -Length 128
```

```powershell
# One Time Passwords

# If you want to return a 4 digit number you can use this:

Get-Password -IncludeNumeric -Length 4
```

## Planned Features, up for grabs :)

```powershell
# Specify your own special characters

# You can specify your own special characters

Get-Password -IncludeLowercase -IncludeUppercase -IncludeNumeric -IncludeSpecial "!%¤/:)"
```