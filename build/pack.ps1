param ([string]$versionSuffix = "",
  [string]$configuration = "Release")
$ErrorActionPreference = "Stop"

# Set location to the Solution directory
(Get-Item $PSScriptRoot).Parent.FullName | Push-Location

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
$commerceMajor = [int]::Parse($commerceParts[0]) + 1
$commerceNextMajorVersion = ($commerceMajor.ToString() + ".0.0") 

[xml] $versionFile = Get-Content "./src/EPiServer.Marketing.KPI/version.props"
$mtVersion = $versionFile.SelectSingleNode("Project/PropertyGroup/VersionPrefix").InnerText + $versionSuffix 
$mtParts = $mtVersion.Split(".")
$mtMajor = [int]::Parse($mtParts[0]) + 1
$mtNextMajorVersion = ($mtMajor.ToString() + ".0.0") 

[xml] $versionFile = Get-Content "./src/EPiServer.Marketing.KPI.Commerce/version.props"
$mtCommerceVersion = $versionFile.SelectSingleNode("Project/PropertyGroup/VersionPrefix").InnerText + $versionSuffix 

[xml] $versionFile = Get-Content "./src/EPIServer.Marketing.Messaging/version.props"
$mtMessageVersion = $versionFile.SelectSingleNode("Project/PropertyGroup/VersionPrefix").InnerText + $versionSuffix 

[xml] $versionFile = Get-Content "./src/EPiServer.Marketing.Testing.Web/version.props"
$version = $versionFile.SelectSingleNode("Project/PropertyGroup/VersionPrefix").InnerText + $versionSuffix 

Remove-Item -Path ./zipoutput -Recurse -Force -Confirm:$false -ErrorAction Ignore

Copy-Item "./src/EPiServer.Marketing.KPI.Commerce/clientResources/dist" -Destination "./zipoutput/EPiServer.Marketing.KPI.Commerce/clientResources/dist" -Recurse
New-Item -Path "./zipoutput/EPiServer.Marketing.KPI.Commerce" -Name "$mtCommerceVersion" -ItemType "directory"

[xml] $moduleFile = Get-Content "./src/EPiServer.Marketing.KPI.Commerce/module.config"
$module = $moduleFile.SelectSingleNode("module")
$module.Attributes["clientResourceRelativePath"].Value = $mtCommerceVersion
$moduleFile.Save("./zipoutput/EPiServer.Marketing.KPI.Commerce/module.config")
Move-Item -Path "./zipoutput/Episerver.Marketing.KPI.Commerce/clientResources" -Destination "./zipoutput/Episerver.Marketing.KPI.Commerce/$mtCommerceVersion/clientresources"

New-Item -Path "./zipoutput/EPiServer.Marketing.Testing" -Name "clientResources" -ItemType "directory"
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/InitializeModule.js" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/TestNotification.js" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/cmsuicomponents" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/cmsuicomponents" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/command" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/command" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/css" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/css" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/nls" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/nls" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/scripts" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/scripts" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/viewmodels" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/viewmodels" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/views" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/views" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/widgets" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/widgets" -Recurse
Copy-Item "./src/EPiServer.Marketing.Testing.Web/clientResources/config/dist" -Destination "./zipoutput/EPiServer.Marketing.Testing/clientResources/config/dist" -Recurse
New-Item -Path "./zipoutput/EPiServer.Marketing.Testing" -Name "$version" -ItemType "directory"
[xml] $moduleFile = Get-Content "./src/EPiServer.Marketing.Testing.Web/module.config"
$module = $moduleFile.SelectSingleNode("module")
$module.Attributes["clientResourceRelativePath"].Value = $version
$moduleFile.Save("./zipoutput/EPiServer.Marketing.Testing/module.config")
Move-Item -Path "./zipoutput/Episerver.Marketing.Testing/clientResources" -Destination "./zipoutput/Episerver.Marketing.Testing/$version/clientresources"
Copy-Item "./src/EPiServer.Marketing.Testing.Web/Images" -Destination "./zipoutput/EPiServer.Marketing.Testing/$version/Images" -Recurse

$compress = @{
  Path = "./zipoutput/Episerver.Marketing.KPI.Commerce/*"
  CompressionLevel = "Optimal"
  DestinationPath = "./zipoutput/Episerver.Marketing.KPI.Commerce.zip"
}
Compress-Archive @compress

$compress = @{
  Path = "./zipoutput/Episerver.Marketing.Testing/*"
  CompressionLevel = "Optimal"
  DestinationPath = "./zipoutput/Episerver.Marketing.Testing.zip"
}
Compress-Archive @compress

Pop-Location

dotnet pack --no-restore --no-build -c $configuration /p:PackageVersion=$version /p:CmsVersion=$cmsVersion /p:CmsNextMajorVersion=$cmsNextMajorVersion /p:UiVersion=$uiVersion /p:UiNextMajorVersion=$uiNextMajorVersion /p:CommerceVersion=$commerceVersion /p:CommerceNextMajorVersion=$commerceNextMajorVersion /p:MtVersion=$mtVersion /p:MtCommerceVersion=$mtCommerceVersion /p:MtMessageVersion=$mtMessageVersion EPiServer.Marketing.Testing.sln

Pop-Location