$outputDir = ".\package\"
$build = "Release"
$version = "11.0.0"

nuget.exe pack ".\src\BVNetwork.404Handler.csproj" -IncludeReferencedProjects -properties Configuration=$build -Version $version -OutputDirectory $outputDir
