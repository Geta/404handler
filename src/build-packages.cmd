@echo off
del *.nupkg
msbuild BVNetwork.404Handler.Cms8.csproj /t:Rebuild /p:Configuration=Release /v:m

msbuild BVNetwork.404Handler.Cms9.csproj /t:Rebuild /p:Configuration=Release /v:m

msbuild BVNetwork.404Handler.csproj /t:Rebuild /p:Configuration=Release /v:m
