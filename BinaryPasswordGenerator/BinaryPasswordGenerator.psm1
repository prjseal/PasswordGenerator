# Get functions
Import-Module "$PSScriptRoot\bin\Debug\netstandard2.0\PasswordGenerator.dll\" -Force


# Aliases
New-Alias -Name 'npwd' -Value 'New-Password'

# Export everything in the directory
Export-ModuleMember -Function * -Cmdlet 'New-Password'  -Alias *