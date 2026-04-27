param(
    [ValidateSet("Restore", "Build", "Test", "TestE2E", "Mutate")]
    [string] $Target = "Test"
)

$ErrorActionPreference = "Stop"

$SolutionFile  = "RedcapApi.slnx"
$TestProject   = "tests/RedcapApi.Tests/RedcapApi.Tests.csproj"
$StrykerProject = "src/RedcapApi"
$DotnetCliHome = Join-Path $PSScriptRoot ".dotnet-home"
$NuGetConfig = Join-Path $PSScriptRoot "NuGet.Config"

if (-not $env:DOTNET_CLI_HOME) {
    New-Item -ItemType Directory -Path $DotnetCliHome -Force | Out-Null
    $env:DOTNET_CLI_HOME = $DotnetCliHome
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Arguments
    )

    & dotnet @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw ("dotnet {0} failed with exit code {1}." -f ($Arguments -join " "), $LASTEXITCODE)
    }
}

switch ($Target) {
    "Restore" {
        Invoke-DotNet @("restore", $SolutionFile, "--configfile", $NuGetConfig)
    }
    "Build" {
        Invoke-DotNet @("restore", $SolutionFile, "--configfile", $NuGetConfig)
        Invoke-DotNet @("build", $SolutionFile, "-c", "Release", "--no-restore")
    }
    "Test" {
        Invoke-DotNet @("restore", $SolutionFile, "--configfile", $NuGetConfig)
        Invoke-DotNet @("build", $SolutionFile, "-c", "Release", "--no-restore")
        Invoke-DotNet @("test", $TestProject, "--filter", "Category!=E2E", "--verbosity", "minimal")
    }
    "TestE2E" {
        Invoke-DotNet @("restore", $SolutionFile, "--configfile", $NuGetConfig)
        Invoke-DotNet @("build", $SolutionFile, "-c", "Release", "--no-restore")
        Invoke-DotNet @("test", $TestProject, "--verbosity", "minimal")
    }
    "Mutate" {
        Invoke-DotNet @("tool", "restore", "--configfile", $NuGetConfig)
        Push-Location $StrykerProject
        try {
            Invoke-DotNet @("stryker", "--config-file", "stryker-config.json")
        }
        finally {
            Pop-Location
        }
    }
}
