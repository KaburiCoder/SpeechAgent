# VoiceMedicAgent — Claude Code 작업 지침

## ⚠️ 소스 파일 인코딩: CP949 (필독)

이 프로젝트의 모든 C# 소스(`*.cs`, `*.xaml`, `*.csproj` 등)는 **CP949(EUC-KR), BOM 없음**으로 저장되어 있으며 한글 주석/문자열이 다수 포함됩니다.

### 문제
Claude Code의 **Edit / Write 도구는 파일을 UTF-8 기준으로 읽고 씁니다.** CP949 소스를 이 도구로 편집하면:
- 편집 시 파일 전체가 UTF-8로 다시 쓰여, 기존 한글 주석이 **복구 불가능하게 깨집니다(U+FFFD).**
- 파일이 CP949+UTF-8 혼합이 되고, BOM 없는 파일에 유효하지 않은 UTF-8 바이트가 생기면 컴파일러(Roslyn) 해석이 흔들려 **런타임 로그/문자열의 한글까지 깨질 수 있습니다.**

### 규칙 — 한글 포함 소스(.cs 등) 수정 시 Edit/Write 도구 직접 사용 금지
대신 **PowerShell로 CP949 인코딩을 유지하며** 수정한다:

```powershell
$cp949 = [System.Text.Encoding]::GetEncoding(949)
$path  = "경로\파일.cs"
# 읽기 (CP949 → 문자열)
$c = $cp949.GetString([System.IO.File]::ReadAllBytes($path))
$c = $c.Replace("`r`n","`n")                 # 매칭 편의를 위해 LF로 정규화
# 치환: old/new 는 @'...'@ (단일인용 here-string) 로 정의해 $ 보간 방지
#   $c = $c.Replace($old, $new)
# 치환 전 정확히 1곳만 매칭되는지 확인 권장:
#   ([regex]::Matches($c,[regex]::Escape($old))).Count
$c = $c.Replace("`n","`r`n")                 # CRLF 복원
# 쓰기 (문자열 → CP949)
[System.IO.File]::WriteAllText($path, $c, $cp949)
```

### 수정 후 반드시 검증
```powershell
$cp949=[System.Text.Encoding]::GetEncoding(949)
$b=[System.IO.File]::ReadAllBytes($path)
$f=0; for($i=0;$i -lt $b.Length-2;$i++){ if($b[$i]-eq 0xEF -and $b[$i+1]-eq 0xBF -and $b[$i+2]-eq 0xBD){$f++} }
"U+FFFD(깨진문자)=$f"                          # 0 이어야 정상
($cp949.GetString($b) -split "`n") | Select-String "수정한_한글_키워드"   # 한글 정상 출력 확인
```
- 깨졌다면 `git checkout -- <파일>` 로 원본(CP949) 복구 후 위 절차로 재작업.

### 예외
- `*.md`, `*.json`, `.editorconfig` 등 **컴파일 대상이 아닌 파일**은 UTF-8 기준이며 일반 Edit/Write 도구 사용 가능.
- 이 `CLAUDE.md` 자체도 UTF-8.

---

## 빌드 / 단일파일 게시
- 단일파일 게시: `dotnet publish -p:PublishProfile=FolderProfile` (win-x64, SelfContained, PublishSingleFile)
- `IncludeAllContentForSelfExtract=true` 이므로 tessdata 등 콘텐츠가 exe에 번들되어, 실행 시 `AppContext.BaseDirectory`(임시 추출폴더 `%TEMP%\.net\VoiceMedicAgent\<해시>\`)로 풀린다. **exe 옆에는 없다.**
- 따라서 번들 콘텐츠(tessdata 등) 경로는 반드시 `AppContext.BaseDirectory` 기준으로 잡는다. (`PathUtils.GetExeDirectory()` 는 exe 옆 파일용이라 단일파일에서 번들 콘텐츠를 못 찾음)

## 로그
- 위치: `{exe폴더}\Log\yyyy-MM-dd.txt` (쓰기 불가 시 폴백 `%LocalAppData%\VoiceMedicAgent\Log\`)
- **레벨 필터 없음** — Debug 포함 전부 파일에 기록됨.
- OCR 검증용 로그 태그: `[OCR]` (tessdata 경로/존재여부 세션 1회, 의사랑 차트 인식 결과 호출당).
