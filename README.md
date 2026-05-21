# SpeechAgent

## Velopack 배포

### 자동 배포 (GitHub Actions)

1. **태그 기반 배포**: 버전 태그를 푸시하면 자동으로 배포됩니다.
   ```bash
   git tag v0.0.3
   git push origin v0.0.3
   ```

2. **수동 배포**: GitHub Actions 페이지에서 "Deploy SpeechAgent with Velopack" 워크플로우를 수동으로 실행할 수 있습니다.

### 로컬 배포

#### Windows
```bash
git add .
git commit -m ".."
git push
git tag v0.0.20
git push origin v0.0.20
```

### 수동 배포 (기존 방법)

https://docs.velopack.io/getting-started/csharp?platform=wpf

**기존 폴더 삭제**
```
Remove-Item -Recurse -Force .\publish
Remove-Item -Recurse -Force .\Releases
```
**publish**
```
# 64bit
dotnet publish SpeechAgent.csproj --self-contained -r win-x64 -o .\publish

# 32bit
dotnet publish SpeechAgent.csproj --self-contained -r win-x86 -o .\publish
```

**pack**
```
vpk pack --packId SpeechAgent --packVersion 0.0.32 --packDir .\publish --mainExe SpeechAgent.exe
```

**publish github release**
```
vpk upload github --repoUrl https://github.com/KaburiCoder/SpeechAgent --publish --releaseName "SpeechAgent 0.0.2" --tag v0.0.2 --token ghp_YOUR_TOKEN
```

## 배포 준비사항

1. **GitHub Token**: GitHub Actions를 위해 repository에 `GITHUB_TOKEN` 권한이 필요합니다 (기본 제공).
2. **Velopack CLI**: 로컬 배포를 위해 `dotnet tool install -g vpk` 명령으로 설치해야 합니다.
3. **권한**: GitHub repository에 Releases 생성 권한이 있어야 합니다.

## 배포 프로세스

1. **빌드**: .NET 8 프로젝트를 win-x64 런타임으로 self-contained 방식으로 빌드
2. **패키징**: Velopack을 사용하여 설치 패키지 생성
3. **업로드**: GitHub Releases에 자동 업로드
4. **아티팩트**: CI/CD 실행 시 빌드 결과물을 아티팩트로 보관 (30일)

## 트러블슈팅

### `vpk` 명령어를 찾을 수 없음

`publish.ps1` 실행 시 다음 에러가 발생하는 경우:

```
vpk : 'vpk' 용어가 cmdlet, 함수, 스크립트 파일 또는 실행할 수 있는 프로그램 이름으로 인식되지 않습니다.
```

**원인**: Velopack CLI(`vpk`)가 글로벌 dotnet tool로 설치되어 있지 않음.

**해결**:
```powershell
dotnet tool install -g vpk
```

설치 후 새 PowerShell 창을 열어야 PATH가 갱신되어 `vpk` 명령이 인식됩니다.

설치 확인:
```powershell
vpk -h
```

> 참고: `vpk` CLI 버전은 `SpeechAgent.csproj`의 `Velopack` NuGet 패키지 버전과 일치시키는 것이 좋습니다 (현재 0.0.1298).