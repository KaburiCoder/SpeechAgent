@echo off
setlocal enabledelayedexpansion

REM csproj 파일에서 VersionPrefix 라인 찾기
for /f "tokens=2 delims=<>" %%A in ('findstr "VersionPrefix" SpeechAgent.csproj') do (
set VERSION=%%A
)

echo Version: !VERSION!
pause