@echo off
setlocal

:: ���� �Է�
set /p VERSION=������ ������ �Է��ϼ��� (��: 0.0.2): 

:: ��ū �Է�
set /p TOKEN=GitHub Token�� �Է��ϼ���: 

:: ���� ����
if exist publish rmdir /s /q publish
if exist Releases rmdir /s /q Releases

:: publish
dotnet publish SpeechAgent.csproj --self-contained -r win-x64 -o .\publish

:: pack
vpk pack --packId SpeechAgent --packVersion %VERSION% --packDir .\publish --mainExe SpeechAgent.exe

:: upload
vpk upload github --repoUrl https://github.com/KaburiCoder/SpeechAgent --publish --releaseName "SpeechAgent %VERSION%" --tag v%VERSION% --token %TOKEN%

echo ���� �Ϸ�!
pause