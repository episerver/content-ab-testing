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
exec "dotnet" "build EPiServer.MarketingAutomationIntegration.sln -c $configuration"

# Run XUnit test projects
#exec "dotnet"  "test EPiServer.MarketingAutomationIntegration.sln -l $logger -v $verbosity -c $configuration --no-build --no-restore"

# Generate Sandcastle Documentation, By default only happens on build machine.
if([System.Convert]::ToBoolean($generateDoc) -eq $true) {
	&"$msbuild" /p:Configuration=Release ..\Documentation\KPI\Kpi.shfbproj
	&"$msbuild" /p:Configuration=Release ..\Documentation\\KPI.Commerce\Kpicommerce.shfbproj
	#&"$msbuild" /p:Configuration=Release ..\Documentation\Messaging\Messaging.shfbproj
	&"$msbuild" /p:Configuration=Release ..\Documentation\Testing\Testing.shfbproj
}

Pop-Location
