@echo off
@setlocal

set MYDIR=%~dp0
pushd "%MYDIR%"

docker build -t nefarius.azurecr.io/wdscp:latest . && docker push nefarius.azurecr.io/wdscp:latest

popd
endlocal