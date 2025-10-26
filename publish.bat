@echo off
setlocal

:: 버전 입력
set /p VERSION=배포할 버전을 입력하세요 (예: 0.0.2): 

:: 토큰 입력
set /p TOKEN=GitHub Token을 입력하세요: 

:: 폴더 삭제
if exist publish rmdir /s /q publish
if exist Releases rmdir /s /q Releases

:: publish
dotnet publish SpeechAgent.csproj --self-contained -r win-x64 -o .\publish

:: pack
vpk pack --packId SpeechAgent --packVersion %VERSION% --packDir .\publish --mainExe SpeechAgent.exe

:: upload
vpk upload github --repoUrl https://github.com/KaburiCoder/SpeechAgent --publish --releaseName "SpeechAgent %VERSION%" --tag v%VERSION% --token %TOKEN%

echo 배포 완료!
pause