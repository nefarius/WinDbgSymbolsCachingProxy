@echo off
@setlocal

set MYDIR=%~dp0
pushd "%MYDIR%"

echo Resulting binaries will be put in .\publish-x64

dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\release-win-x64.pubxml -r win-x64 .\server\WinDbgSymbolsCachingProxy.csproj
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\release-win-x64.pubxml -r win-x64 .\agent\HarvestingAgent.csproj

popd
endlocal