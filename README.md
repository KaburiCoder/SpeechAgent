# Velopack 배포

https://docs.velopack.io/getting-started/csharp?platform=wpf

**publish**
```
dotnet publish SpeechAgent.csproj --self-contained -r win-x64 -o .\publish
```

**pack**
```
vpk pack --packId SpeechAgent --packVersion 0.0.2 --packDir .\publish --mainExe SpeechAgent.exe
```

**publish github release**
```
vpk upload github --repoUrl https://github.com/KaburiCoder/SpeechAgent --publish --releaseName "SpeechAgent 0.0.2" --tag v0.0.2 --token ghp_
```