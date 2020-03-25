$outputDir = ".\package\"
$build = "Release"
$version = "11.4.0"

nuget.exe pack ".\src\Geta.404Handler\Geta.404Handler.csproj" -IncludeReferencedProjects -properties Configuration=$build -Version $version -OutputDirectory $outputDir
