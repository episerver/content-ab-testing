# NOTE: This script must currently be executed from the solution dir (due to specs)
Param([string] $configuration = "Release", [string] $logger="trx", [string] $verbosity="normal")
$ErrorActionPreference = "Stop"

# Set location to the Solution directory
(Get-Item $PSScriptRoot).Parent.FullName | Push-Location

Import-Module .\build\exechelper.ps1

&"dotnet" restore ..\EPiServer.Marketing.Testing.sln --packages ..\packages


# Install .NET tooling
exec .\build\dotnet-cli-install.ps1

# Build dotnet projects
exec "dotnet" "build EPiServer.Marketing.Testing.sln -c $configuration"

# Run XUnit test projects
#exec "dotnet"  "test EPiServer.Marketing.Testing.sln -l $logger -v $verbosity -c $configuration --no-build --no-restore"


Pop-Location
