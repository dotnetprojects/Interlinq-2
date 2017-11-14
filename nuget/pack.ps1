$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'

Write-Host "root: $root"

$version = [System.Reflection.Assembly]::LoadFile("$root\InterLinq\bin\Debug\InterLinq.dll").GetName().Version
$versionStr = "{0}.{1}.{2}" -f ($version.Major, $version.Minor, $version.Build)

Write-Host "Setting .nuspec version tag to $versionStr"

$content = (Get-Content $root\NuGet\Package.nuspec) 
$content = $content -replace '\$version\$',$versionStr

$content | Out-File $root\nuget\Package.compiled.nuspec

& $root\NuGet\NuGet.exe pack $root\nuget\Package.compiled.nuspec

$content = (Get-Content $root\NuGet\Package2.nuspec) 
$content = $content -replace '\$version\$',$versionStr

$content | Out-File $root\nuget\Package2.compiled.nuspec

& $root\NuGet\NuGet.exe pack $root\nuget\Package2.compiled.nuspec