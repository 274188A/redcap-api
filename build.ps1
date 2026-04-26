param(
    [ValidateSet("Restore", "Build", "Test", "TestE2E")]
    [string] $Target = "Test"
)

$ErrorActionPreference = "Stop"

$SolutionFile  = "RedcapApi.slnx"
$TestProject   = "tests/RedcapApi.Tests/RedcapApi.Tests.csproj"

switch ($Target) {
    "Restore" {
        dotnet restore $SolutionFile
    }
    "Build" {
        dotnet restore $SolutionFile
        dotnet build $SolutionFile -c Release --no-restore
    }
    "Test" {
        dotnet restore $SolutionFile
        dotnet build $SolutionFile -c Release --no-restore
        dotnet test $TestProject --filter "Category!=E2E" --verbosity minimal
    }
    "TestE2E" {
        dotnet restore $SolutionFile
        dotnet build $SolutionFile -c Release --no-restore
        dotnet test $TestProject --verbosity minimal
    }
}
