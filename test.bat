@echo off
for /f "delims=" %%a in ('findstr "VersionPrefix" SpeechAgent.csproj') do (
 set "line=%%a"
 call set "ver=%%line:*<VersionPrefix>=%%"
 call set "ver=%%ver:</VersionPrefix>=%%"
 echo VersionPrefix: %ver%
)