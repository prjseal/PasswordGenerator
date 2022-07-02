# Get functions
Import-Module "$PSScriptRoot\PasswordGenerator.dll" -Force

# Aliases
New-Alias -Name 'gpwd' -Value "Get-Password"

# Export everything in the directory
Export-ModuleMember -Function * -Cmdlet * -Alias *