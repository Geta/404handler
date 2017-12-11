@echo off
del *.nupkg
msbuild BVNetwork.404Handler.csproj /t:Rebuild /p:Configuration=Release /v:m
