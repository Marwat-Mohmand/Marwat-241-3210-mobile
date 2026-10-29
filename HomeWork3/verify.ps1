$ErrorActionPreference = 'Stop'
$previousResolver = $env:MSBuildEnableWorkloadResolver
$env:MSBuildEnableWorkloadResolver = 'false'
$config = Join-Path $PSScriptRoot 'NuGet.Offline.Config'
try {
    dotnet build (Join-Path $PSScriptRoot 'prjPasswordCreator/prjPasswordCreator.Desktop.slnx') --configfile $config --nologo -m:1 -nr:false -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw 'Desktop build failed.' }
    dotnet run --project (Join-Path $PSScriptRoot 'prjPasswordCreator/PasswordCreator.Tests') --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Checks failed.' }
}
finally {
    $env:MSBuildEnableWorkloadResolver = $previousResolver
}
