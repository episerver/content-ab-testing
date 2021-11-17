param ([string]$version = "",
  [string]$mtVersion = "",
  [string]$mtCommerceVersion = "",
  [string]$mtMessageVersion = "",
  [string]$configuration = "Release")
$ErrorActionPreference = "Stop"

# Set location to the Solution directory
(Get-Item $PSScriptRoot).Parent.FullName | Push-Location

Import-Module .\build\exechelper.ps1

# Install .NET tooling
exec .\build\dotnet-cli-install.ps1

[xml] $versionFile = Get-Content "./build/DependencyVersions.props"
$node = $versionFile.SelectSingleNode("Project/PropertyGroup/CmsCoreVersionCommon")
$cmsVersion = $node.InnerText
$parts = $cmsVersion.Split(".")
$major = [int]::Parse($parts[0]) + 1
$cmsNextMajorVersion = ($major.ToString() + ".0.0") 

$uiNode = $versionFile.SelectSingleNode("Project/PropertyGroup/CmsUiVersionCommon")
$uiVersion = $uiNode.InnerText
$uiParts = $uiVersion.Split(".")
$uiMajor = [int]::Parse($uiParts[0]) + 1
$uiNextMajorVersion = ($uiMajor.ToString() + ".0.0") 

$commerceNode = $versionFile.SelectSingleNode("Project/PropertyGroup/CmsCommerceVersionCommon")
$commerceVersion = $commerceNode.InnerText
$commerceParts = $commerceVersion.Split(".")
$commerceMajor = [int]::Parse($commParts[0]) + 1
$commerceNextMajorVersion = ($commerceMajor.ToString() + ".0.0") 

$mtParts = $mtVersion.Split(".")
$mtMajor = [int]::Parse($mtParts[0]) + 1
$mtNextMajorVersion = ($mtMajor.ToString() + ".0.0") 

# Packaging public packages
exec "dotnet" "pack --no-restore --no-build -c $configuration /p:PackageVersion=$version /p:CmsVersion=$cmsVersion /p:CmsNextMajorVersion=$cmsNextMajorVersion /p:UiVersion=$uiVersion /p:UiNextMajorVersion=$uiNextMajorVersion /p:CommerceVersion=$commerceVersion /p:CommerceNextMajorVersion=$commerceNextMajorVersion /p:MtVersion=$mtVersion /p:MtCommerceVersion=$mtCommerceVersion /p:MtMessageVersion=$mtMessageVersion EPiServer.Marketing.Testing.sln"

Pop-Location