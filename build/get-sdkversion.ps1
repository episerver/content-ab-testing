$solutionDir = (get-item $PSScriptRoot).parent.FullName

$globalJson = Join-Path $solutionDir "global.json"
$regexp = new-object System.Text.RegularExpressions.Regex('version\"\s*:\s*\"(.*?)(?=\")')
$content = Get-Content -Path $globalJson
$version = $regexp.Matches($content).Groups[1].Value
if ($version.EndsWith("-*"))
{
	return $version.SubString(0, $version.Length -2)
}
else
{
	return $version
}