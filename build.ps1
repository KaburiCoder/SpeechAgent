# 폴더 정리
Write-Host "[정리] 기존 폴더 삭제 중..."
if (Test-Path "publish") { Remove-Item "publish" -Recurse -Force }
if (Test-Path "Releases") { Remove-Item "Releases" -Recurse -Force }
Write-Host "✓ 폴더 정리 완료"

# 64비트 빌드
Write-Host "[빌드] 64비트 빌드 시작..."
dotnet publish SpeechAgent.csproj --self-contained -p:PublishSingleFile=true -r win-x64 -o .\publish

# 64비트 팩 및 업로드
# Write-Host "[팩] 64비트 팩 생성..."
# vpk pack --packId VoiceMedicAgent --packVersion 0.0.63 --packDir .\publish --mainExe SpeechAgent.exe