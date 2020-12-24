$outputDir = ".\package\"
$build = "Release"
$version = "1.2.1"

.\.nuget\nuget.exe pack ".\src\Geta.404Handler\Geta.404Handler.csproj" -IncludeReferencedProjects -properties Configuration=$build -Version $version -OutputDirectory $outputDir
