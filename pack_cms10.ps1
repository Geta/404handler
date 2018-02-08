$outputDir = ".\package\"
$build = "Release"
$version = "10.2.0"

nuget.exe pack ".\src\BVNetwork.404Handler.Cms10.csproj" -IncludeReferencedProjects -properties Configuration=$build -Version $version -OutputDirectory $outputDir
