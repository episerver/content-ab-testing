Param([String]$WindowsSDKPath)

$rootDir = Get-Location
$srcProjects = [Array](Get-ChildItem -Directory -Path $rootDir) 
$signError = $false
$assemblies = @()

foreach($item in $srcProjects)
{
    $assemblies = $assemblies + (Get-ChildItem -Recurse -Path (Join-Path ($rootDir) ".\$($item.Name)") -File -Filter ($item.Name + ".dll") -Exclude *Sources.dll)
}

foreach ($assembly in $assemblies)
{
   Write-Host ("Signing " + $assembly.FullName)
   $LASTEXITCODE = 0
   &"$WindowsSDKPath\sn.exe" -q -Rc  $assembly.FullName "EPiServerProduct"
   if ($LASTEXITCODE -ne 0)
   {
       exit $LASTEXITCODE
   }
}

$url = "http://timestamp.digicert.com/scripts/timstamp.dll"
$cert = (Get-ChildItem -Recurse -File cert:\ | Where-Object {$_.Thumbprint -match "EABEA65470D9B25F92B09C202EB3DED15FD0B0A9"})[0]
foreach($item in $assemblies)
{
    Write-Host ("Authenticode signing " + $item.FullName)
    Set-AuthenticodeSignature -FilePath $item.FullName -Certificate $cert -TimestampServer $url -WarningAction Stop | Out-Null

    $signed = (Get-AuthenticodeSignature -filepath $item.FullName).Status
    if($signed -eq "NotSigned") 
    { 
        Write-Host ("Authenticode signing failed " + $item.FullName)
        exit 1
    }
}