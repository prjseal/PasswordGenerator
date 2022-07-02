---
external help file: PasswordGenerator.dll-Help.xml
Module Name: BinaryPasswordGenerator
online version:
schema: 2.0.0
---

# Get-Password

## SYNOPSIS
 A port of the .NET Standard library "Password Generator" which generates random passwords with different settings to meet the OWASP requirements  

## SYNTAX

```
Get-Password [-Length <Int32>] [-Amount <Int32>] [-IncludeSpecial] [-IncludeNumeric] [-IncludeUppercase]
 [-IncludeLowercase] [<CommonParameters>]
```

## DESCRIPTION
Generates random passwords based on parameters

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-Password
```

Will return a random password with the default settings, 16 length

### Example 2
```powershell
PS C:\> Get-Password -Length 32
```

Will return a password which is 32 characters long

### Example 3
```powershell
PS C:\> Get-Password -IncludeLowercase -IncludeUppercase -Length 21
```

 Will return a password which only contains lowercase and uppercase characters and is 21 characters long 

### Example 4
```powershell
PS C:\>  Get-Password -IncludeNumeric
```

 This will return a password which is just numbers and has a default length of 16 

### Example 5
```powershell
PS C:\>  Get-Password -IncludeLowercase -IncludeUppercase -IncludeSpecial
```

 As above, here is how to get lower, upper and special characters using this approach 

### Example 6
```powershell
PS C:\>  Get-Password -IncludeLowercase -IncludeUppercase -IncludeSpecial -Length 128
```

 This is the same as the above, but with a length of 128 

### Example 7
```powershell
PS C:\>  Get-Password -IncludeNumeric -Length 4
```

 Returns a 4 digit number, used for PIN or One TIme Passwords 

## PARAMETERS

### -Amount
 The amount of passwords to return 

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeLowercase
 Include Lowercase characters 

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeNumeric
 Include Numeric characters 

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeSpecial
 Include Special characters 

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeUppercase
 Include uppercase characters  

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Length
 Password Length, default is 16 

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
