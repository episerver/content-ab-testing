Param([string]$branchName, [Int]$buildCounter, [String]$publishPackages)

if (!$branchName -or !$buildCounter) {
    Write-Error "`$branchName and `$buildCounter parameters must be supplied"
    exit 1
}

Function GetVersion($path) {
    [xml] $versionFile = Get-Content $path
    return $versionFile.SelectSingleNode("Project/PropertyGroup/VersionPrefix").InnerText
}

$addonHelperAssemblyVersion = GetVersion "src\EPiServer.AddOns.Helpers\EPiServer.AddOns.Helpers.csproj"

if (!$addonHelperAssemblyVersion) {
   $addonHelperAssemblyVersion = "1.0.0"
}

$cryptoKeyVaultAssemblyVersion = GetVersion "src\EPiServer.Forms.Crypto.AzureKeyVault\EPiServer.Forms.Crypto.AzureKeyVault.csproj"

if (!$cryptoKeyVaultAssemblyVersion) {
   $cryptoKeyVaultAssemblyVersion = "2.0.0"
}

$formsAssemblyVersion = GetVersion "build\version.props"

if (!$formsAssemblyVersion) {
   $formsAssemblyVersion = "5.0.0"
}

switch -wildcard ($branchName) {
    "master" { 
        $preReleaseInfo = ""
        $publishPackages = "True"
    }
    "develop" { 
        $preReleaseInfo = "-inte-{0:D6}"
        $publishPackages = "True"
    }
    "bugfix/*" { 
        $preReleaseInfo = "-ci-{0:D6}"
        $publishPackages = "True"
    }
    "hotfix/*" { 
        $preReleaseInfo = ""
        $publishPackages = "True"
    }
    "release/*" { 
        $preReleaseInfo = "-pre-{0:D6}"
        $publishPackages = "True"
    }
    "feature/*" { 
        $isMatch = $branchName -match ".*/([A-Z]+-[\d]+)-"
        if ($isMatch -eq $TRUE) {
            $feature = $Matches[1]
            $preReleaseInfo = "-feature-$feature-{0:D6}"
            $publishPackages = "True"
        }
        else {
            $preReleaseInfo = "-feature-{0:D6}" 
        }
    }
    default { $preReleaseInfo = "-ci-{0:D6}" } 
}

$addonHelperInformationalVersion  = "$addonHelperAssemblyVersion$preReleaseInfo" -f $buildCounter
 
"EPiServer.AddOns.Helpers AssemblyVersion: $addonHelperAssemblyVersion"
"EPiServer.AddOns.Helpers AssemblyInformationalVersion: $addonHelperInformationalVersion"

$cryptoKeyVaultInformationalVersion  = "$cryptoKeyVaultAssemblyVersion$preReleaseInfo" -f $buildCounter
 
"EPiServer.Forms.Crypto.AzureKeyVault AssemblyVersion: $cryptoKeyVaultAssemblyVersion"
"EPiServer.Forms.Crypto.AzureKeyVault AssemblyInformationalVersion: $cryptoKeyVaultInformationalVersion"

$formsInformationalVersion = "$formsAssemblyVersion$preReleaseInfo" -f $buildCounter

"EPiServer.Forms AssemblyVersion: $formsAssemblyVersion"
"EPiServer.Forms AssemblyInformationalVersion: $formsInformationalVersion"

"##teamcity[setParameter name='addonHelperVersion' value='$addonHelperInformationalVersion']"
"##teamcity[setParameter name='cryptoKeyVaultVersion' value='$cryptoKeyVaultInformationalVersion']"
"##teamcity[setParameter name='formsVersion' value='$formsInformationalVersion']"
"##teamcity[setParameter name='publishPackages' value='$publishPackages']"

#Set DNX_BUILD_VERSION for dnx compiling (without dash)
$dnxPreReleaseInfo = ""
if ($preReleaseInfo.Length -gt 0) {
    $dnxPreReleaseInfo = ($preReleaseInfo.Substring(1)) -f $buildCounter
}
"##teamcity[setParameter name='buildSuffix' value='$dnxPreReleaseInfo']"
"##teamcity[setParameter name='env.DNX_BUILD_VERSION' value='$dnxPreReleaseInfo']"