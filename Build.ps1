param(
    [Parameter(Mandatory = $false)][string] $Configuration = "Release",
    [Parameter(Mandatory = $false)][string] $OutputPath = "",
    [Parameter(Mandatory = $false)][bool]   $RunTests = $true,
    [Parameter(Mandatory = $false)][bool]   $CreatePackages = $true
)

$ErrorActionPreference = "Stop"

$solutionPath = Split-Path $MyInvocation.MyCommand.Definition
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | ConvertFrom-Json).sdk.version

if ($OutputPath -eq "") {
    $OutputPath = "$(Convert-Path "$PSScriptRoot")\artifacts"
}

$installDotNetSdk = $false;

if (($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue))) {
    Write-Host "The .NET Core SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Host "The required version of the .NET Core SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk -eq $true) {
    $env:DOTNET_INSTALL_DIR = "$(Convert-Path "$PSScriptRoot")\.dotnetcli"

    if (!(Test-Path $env:DOTNET_INSTALL_DIR)) {
        mkdir $env:DOTNET_INSTALL_DIR | Out-Null
        $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"
        Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
        & $installScript -Version "$dotnetVersion" -InstallDir "$env:DOTNET_INSTALL_DIR" -NoPath
    }

    $env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
    $dotnet = "$env:DOTNET_INSTALL_DIR\dotnet"
}
else {
    $dotnet = "dotnet"
}

function DotNetBuild {
    param([string]$Project, [string]$Configuration, [string]$Framework)
    & $dotnet build $Project --output (Join-Path $OutputPath $Framework) --framework $Framework --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

function DotNetTestFullFramework {
  param([string]$Project)
    Write-Host "Testing $Project on .NET full framework..." -ForegroundColor Green

    if ($installDotNetSdk -eq $true) {
        $dotnetPath = $dotnet
    }
    else {
        $dotnetPath = (Get-Command "dotnet.exe").Source
    }

    & $dotnetPath test $Project --framework net471
  }

function DotNetTestWithCoverage {
    Write-Host "Testing $Project on .NET Core with coverage..." -ForegroundColor Green

    if ($installDotNetSdk -eq $true) {
        $dotnetPath = $dotnet
    }
    else {
        $dotnetPath = (Get-Command "dotnet.exe").Source
    }

    $nugetPath = Join-Path $env:USERPROFILE ".nuget\packages"

    $openCoverVersion = "4.7.922"
    $openCoverPath = Join-Path $nugetPath "OpenCover\$openCoverVersion\tools\OpenCover.Console.exe"

    $reportGeneratorVersion = "4.0.11"
    $reportGeneratorPath = Join-Path $nugetPath "ReportGenerator\$reportGeneratorVersion\tools\netcoreapp2.0\ReportGenerator.dll"

    $coverageOutput = Join-Path $OutputPath "code-coverage.xml"
    $reportOutput = Join-Path $OutputPath "coverage"

    & $openCoverPath `
        `"-target:$dotnetPath`" `
        `"-targetargs:test $Project --output $OutputPath --framework netcoreapp2.1`" `
        -output:$coverageOutput `
        `"-excludebyattribute:System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage*`" `
        -hideskipped:All `
        -mergebyhash `
        -mergeoutput `
        -oldstyle `
        -register:user `
        -skipautoprops `
        `"-filter:+[NLog.StructuredLogging.Json]* -[*Test*]*`"

    & $dotnet `
        $reportGeneratorPath `
        `"-reports:$coverageOutput`" `
        `"-targetdir:$reportOutput`" `
        -reporttypes:HTML`;Cobertura `
        -verbosity:Warning

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

function DotNetPack {
    param([string]$Project, [string]$Configuration)
    & $dotnet pack $Project --output $OutputPath --configuration $Configuration --include-symbols --include-source
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed with exit code $LASTEXITCODE"
    }
}

$projects = @(
    (Join-Path $solutionPath "src\NLog.StructuredLogging.Json\NLog.StructuredLogging.Json.csproj")
)

$testProjects = @(
    (Join-Path $solutionPath "src\NLog.StructuredLogging.Json.Tests\NLog.StructuredLogging.Json.Tests.csproj")
)

$packageProjects = @(
    (Join-Path $solutionPath "src\NLog.StructuredLogging.Json\NLog.StructuredLogging.Json.csproj")
)

Write-Host "Building $($projects.Count) projects..." -ForegroundColor Green
ForEach ($project in $projects) {
    DotNetBuild $project $Configuration "netstandard2.0"
    DotNetBuild $project $Configuration "net452"
}

if ($RunTests -eq $true) {
    Write-Host "Testing $($testProjects.Count) project(s)..." -ForegroundColor Green
    ForEach ($project in $testProjects) {
        DotNetTestWithCoverage $project
        DotNetTestFullFramework $project
    }
}

if ($CreatePackages -eq $true) {
    Write-Host "Creating $($packageProjects.Count) package(s)..." -ForegroundColor Green
    ForEach ($project in $packageProjects) {
        DotNetPack $project $Configuration
    }
}
