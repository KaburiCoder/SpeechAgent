# PowerShell 스크립트로 버전 읽기 및 증가
[xml]$csproj = Get-Content "SpeechAgent.csproj"
$currentVersion = $csproj.Project.PropertyGroup.VersionPrefix
Write-Host "현재 버전: $currentVersion"

# 버전 분석 (예: 0.0.32)
$versionParts = $currentVersion -split '\.'
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
$patch = [int]$versionParts[2]

# 3번째 숫자 +1
$patch++
$newVersion = "$major.$minor.$patch"
Write-Host "새 버전: $newVersion"

# .csproj 파일에서 VersionPrefix 업데이트
$csproj.Project.PropertyGroup.VersionPrefix = $newVersion
$csproj.Save("SpeechAgent.csproj")
Write-Host "✓ SpeechAgent.csproj 업데이트됨"

# GitHub Token 입력
$token = Read-Host "GitHub Token을 입력하세요"

# ========== 64비트 배포 ==========
Write-Host "`n========== [64비트] 빌드 및 배포 시작 ==========" -ForegroundColor Cyan

# 폴더 정리
Write-Host "[정리] 기존 폴더 삭제 중..."
if (Test-Path "publish") { Remove-Item "publish" -Recurse -Force }
if (Test-Path "Releases") { Remove-Item "Releases" -Recurse -Force }
Write-Host "✓ 폴더 정리 완료"

# 64비트 빌드
Write-Host "[빌드] 64비트 빌드 시작..."
dotnet publish SpeechAgent.csproj --self-contained -r win-x64 -o .\publish

# 64비트 팩 및 업로드
Write-Host "[팩] 64비트 팩 생성..."
vpk pack --packId VoiceMedicAgent --packVersion $newVersion --packDir .\publish --mainExe SpeechAgent.exe

Write-Host "[업로드] 64비트 배포 중..."
vpk upload github --repoUrl https://github.com/clickcns/SpeechAgent_x64 --publish --releaseName "SpeechAgent $newVersion (x64)" --tag v$newVersion --token $token

Write-Host "✓ 64비트 배포 완료!`n"

# ========== 32비트 배포 ==========
Write-Host "========== [32비트] 빌드 및 배포 시작 ==========" -ForegroundColor Cyan

# 폴더 정리
Write-Host "[정리] 기존 폴더 삭제 중..."
if (Test-Path "publish") { Remove-Item "publish" -Recurse -Force }
if (Test-Path "Releases") { Remove-Item "Releases" -Recurse -Force }
Write-Host "✓ 폴더 정리 완료"

# 32비트 빌드
Write-Host "[빌드] 32비트 빌드 시작..."
dotnet publish SpeechAgent.csproj --self-contained -r win-x86 -o .\publish

# 32비트 팩 및 업로드
Write-Host "[팩] 32비트 팩 생성..."
vpk pack --packId VoiceMedicAgent --packVersion $newVersion --packDir .\publish --mainExe SpeechAgent.exe

Write-Host "[업로드] 32비트 배포 중..."
vpk upload github --repoUrl https://github.com/clickcns/SpeechAgent_x32 --publish --releaseName "SpeechAgent $newVersion (x86)" --tag v$newVersion --token $token

Write-Host "✓ 32비트 배포 완료!`n"

# ========== 완료 ==========
Write-Host "========== 배포 완료! ==========" -ForegroundColor Green
Write-Host "64비트: https://github.com/clickcns/SpeechAgent_x64"
Write-Host "32비트: https://github.com/clickcns/SpeechAgent_x32"
Write-Host "버전: $newVersion"

Read-Host "`n엔터를 눌러 종료하세요"
