using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Constants;
using SpeechAgent.Database;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using SpeechAgent.Features.Settings.FindWin;
using SpeechAgent.Features.Settings.FindWin.Services;
using SpeechAgent.Features.UpdateHistory;
using SpeechAgent.Services;
using SpeechAgent.Services.Api;
using SpeechAgent.Services.NamedPipe;
using SpeechAgent.Utils;
using SpeechAgent.Utils.Automation;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows;
using Velopack;

namespace SpeechAgent
{
  public partial class App : System.Windows.Application
  {
    public static new App Current => (App)System.Windows.Application.Current;
    public IServiceProvider Services { get; } = default!;

    private static IServiceProvider ConfigureServices()
    {
      var services = new ServiceCollection();

      // Singletons
      services.AddHttpClient(
        "SpeechServer",
        client =>
        {
          var settingsService = Current.Services.GetRequiredService<ISettingsService>();

          client.BaseAddress = new Uri(ApiConfig.SpeechBaseUrl);
          client.DefaultRequestHeaders.Add(
            ApiConfig.SpeechUserKey,
            settingsService.Settings.ConnectKey
          );
        }
      );
      services.AddSingleton<HttpClient>();
      services.AddSingleton<IViewService, ViewService>();
      services.AddSingleton<IViewModelFactory, ViewModelFactory>();
      services.AddSingleton<IPatientSearchService, PatientSearchService>();
      services.AddSingleton<ISettingsService, SettingsService>();
      services.AddSingleton<TrayIconService>();
      services.AddSingleton<IUpdateService, UpdateService>();
      services.AddSingleton<IAutoStartService, AutoStartService>();
      services.AddSingleton<IUserNotificationService, UserNotificationService>();
      services.AddSingleton<INamedPipeService, NamedPipeService>();

      // Views
      services.AddSingleton<MainView>();

      // ViewModels
      services.AddTransient<MainViewModel>();
      services.AddTransient<SettingsViewModel>();
      services.AddTransient<FindWinViewModel>();
      services.AddTransient<FindWinApiViewModel>();
      services.AddTransient<FindWinImageViewModel>();
      services.AddTransient<UpdateHistoryViewModel>();

      // Services
      services.AddTransient<IMainService, MainService>();
      services.AddTransient<IWindowCaptureService, WindowCaptureService>();
      services.AddTransient<IAutomationControlSearcher, AutomationControlSearcher>();
      services.AddTransient<IControlSearcher, ControlSearcher>();
      services.AddTransient<IClickSoftControlSearchService, ClickSoftControlSearchService>();
      services.AddTransient<IUpdateHistoryService, UpdateHistoryService>();

      services.AddTransient<ILlmApi, LlmApi>();
      services.AddTransient<IUserNotificationsApi, UserNotificationsApi>();

      return services.BuildServiceProvider();
    }

    static Mutex? _mutex = null;
    private const string MutexName = "VoiceMedicAgent_UniqueMutex";

    private HeartbeatLogger? _heartbeat;

    [STAThread]
    public static void Main()
    {
      // 전역 예외 핸들러를 가장 먼저 등록 — 이후 모든 무성 예외를 잡기 위함
      RegisterGlobalExceptionHandlers();

      LogUtils.WriteSessionBanner();
      LogUtils.WriteLog(LogLevel.Info, "Main() 진입");

      try
      {
        LogUtils.WriteLog(LogLevel.Debug, "기존 프로세스 정리 시작");
        KillLegacyProcesses();

        // 작업 디렉토리를 실행 파일 위치로 변경(윈도우 재시작 후 바로가기로 실행 시 문제 방지)
        Directory.SetCurrentDirectory(PathUtils.GetExeDirectory());
        LogUtils.WriteLog(LogLevel.Debug, $"작업 디렉토리 설정 완료: {Directory.GetCurrentDirectory()}");

        // 관리자 권한 확인 및 필요시 재실행
        LogUtils.WriteLog(LogLevel.Debug, $"관리자 권한 체크 (IsAdmin={AdminHelper.IsRunningAsAdmin()})");
        AdminHelper.RequireAdminOrExit();

        // EUC-KR, CP949, 949
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        LogUtils.WriteLog(LogLevel.Debug, "코드페이지 인코딩 등록 완료");

        // 실행 중인 프로세스 버전 비교 및 처리
        LogUtils.WriteLog(LogLevel.Debug, "Mutex 획득 시도");
        if (!TryAcquireMutex())
        {
          LogUtils.WriteLog(LogLevel.Info, "Mutex 획득 실패 → 기존 프로세스 처리 분기");
          HandleExistingProcess();
          return;
        }
        LogUtils.WriteLog(LogLevel.Info, "Mutex 획득 성공 → RunApp 진입");

        RunApp();
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "Main()에서 처리되지 않은 예외", ex);
        throw;
      }
      finally
      {
        LogUtils.WriteLog(LogLevel.Info, "Main() 종료");
      }
    }

    /// <summary>
    /// 전역 예외 핸들러를 등록합니다. 프로세스가 "떠 있지만 동작 안하는" 상태의 무성 예외를 잡기 위함.
    /// </summary>
    private static void RegisterGlobalExceptionHandlers()
    {
      AppDomain.CurrentDomain.UnhandledException += (s, e) =>
      {
        var ex = e.ExceptionObject as Exception;
        LogUtils.WriteLog(LogLevel.Error, $"AppDomain.UnhandledException (IsTerminating={e.IsTerminating})", ex);
      };

      System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
      {
        LogUtils.WriteLog(LogLevel.Error, "TaskScheduler.UnobservedTaskException", e.Exception);
        e.SetObserved();
      };
    }

    private static void KillLegacyProcesses()
    {
      var processes = Process.GetProcessesByName("SpeechAgent");
      foreach (var process in processes)
      {
        try
        {
          process.Kill();
        }
        catch (Exception ex)
        {
          LogUtils.WriteLog(LogLevel.Error, $"기존 프로세스 종료 실패: {ex.Message}");
        }
      }
    }

    private const int MigrationTimeoutMs = 15000;       // 전체 마이그레이션 타임아웃
    private const int CommandTimeoutSeconds = 5;         // 각 SQL 명령 타임아웃

    private static readonly string[] SqliteSidecarSuffixes = { "", "-wal", "-shm", "-journal" };

    private static void RunApp()
    {
      // 데이터베이스 마이그레이션 적용 — 타임아웃 + 실패 시 DB 파일 삭제 후 재시도
      string dbPath = AppDbContext.GetDbPath();
      LogUtils.WriteLog(LogLevel.Debug, $"DB 경로: {dbPath}");

      if (!TryMigrate())
      {
        LogUtils.WriteLog(LogLevel.Warn, "1차 마이그레이션 실패 — DB 파일 삭제 후 재시도");
        // 풀에 남은 SQLite 연결 핸들을 해제해야 DB 파일 삭제가 가능
        try { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); } catch { }
        DeleteSqliteFiles(dbPath);

        if (!TryMigrate())
        {
          var ex = new InvalidOperationException("DB 마이그레이션이 재시도 후에도 실패했습니다.");
          LogUtils.WriteLog(LogLevel.Error, "DB 초기화 실패 (재시도 포함)", ex);
          throw ex;
        }
        LogUtils.WriteLog(LogLevel.Info, "DB 재생성 후 마이그레이션 성공");
      }

      // WPF Application 실행
      try
      {
        LogUtils.WriteLog(LogLevel.Debug, "App 인스턴스 생성");
        var app = new App();
        LogUtils.WriteLog(LogLevel.Debug, "InitializeComponent 호출");
        app.InitializeComponent();

        // WPF Dispatcher 예외 핸들러는 Application 인스턴스가 생긴 다음에 등록 가능
        app.DispatcherUnhandledException += (s, e) =>
        {
          LogUtils.WriteLog(LogLevel.Error, "Application.DispatcherUnhandledException", e.Exception);
          e.Handled = true; // 앱이 죽지 않도록 막아 진단 가능 상태 유지
        };

        LogUtils.WriteLog(LogLevel.Info, "app.Run() 시작");
        app.Run();
        LogUtils.WriteLog(LogLevel.Info, "app.Run() 정상 반환");
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "WPF 애플리케이션 실행 실패", ex);
        throw;
      }
    }

    /// <summary>
    /// Migrate를 작업 스레드에서 실행하고 MigrationTimeoutMs 안에 끝나지 않으면 실패로 간주합니다.
    /// 각 SQL 명령은 CommandTimeoutSeconds로 추가 제한됩니다.
    /// </summary>
    private static bool TryMigrate()
    {
      AppDbContext? context = null;
      try
      {
        LogUtils.WriteLog(LogLevel.Debug, $"DB 마이그레이션 시도 (전체 타임아웃 {MigrationTimeoutMs}ms, 명령 타임아웃 {CommandTimeoutSeconds}s)");
        context = new AppDbContext();
        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(CommandTimeoutSeconds));

        var ctx = context;
        var migrateTask = Task.Run(() => ctx.Database.Migrate());

        if (!migrateTask.Wait(MigrationTimeoutMs))
        {
          LogUtils.WriteLog(LogLevel.Error, $"DB 마이그레이션 타임아웃 ({MigrationTimeoutMs}ms 초과) — 강제 연결 종료");
          // 작업 스레드가 SQLite 호출에 묶여 있을 가능성 → 연결 강제 종료로 풀어줌
          try { ctx.Database.GetDbConnection().Close(); } catch { }
          return false;
        }

        if (migrateTask.IsFaulted)
        {
          LogUtils.WriteLog(LogLevel.Error, "DB 마이그레이션 실패", migrateTask.Exception?.GetBaseException());
          return false;
        }

        LogUtils.WriteLog(LogLevel.Info, "DB 마이그레이션 완료");
        return true;
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "DB 마이그레이션 중 예외", ex);
        return false;
      }
      finally
      {
        try { context?.Dispose(); } catch { }
      }
    }

    /// <summary>
    /// settings.db 와 관련 SQLite 보조 파일(.db-wal, .db-shm, .db-journal)을 모두 삭제합니다.
    /// 락 경합으로 즉시 삭제 실패 시 200ms 간격으로 5회 재시도합니다.
    /// </summary>
    private static void DeleteSqliteFiles(string dbPath)
    {
      foreach (var suffix in SqliteSidecarSuffixes)
      {
        var file = dbPath + suffix;
        if (!File.Exists(file))
          continue;

        for (int attempt = 1; attempt <= 5; attempt++)
        {
          try
          {
            File.Delete(file);
            LogUtils.WriteLog(LogLevel.Warn, $"DB 파일 삭제: {Path.GetFileName(file)}");
            break;
          }
          catch (Exception ex)
          {
            if (attempt == 5)
              LogUtils.WriteLog(LogLevel.Error, $"DB 파일 삭제 실패 (5회 시도): {file}", ex);
            else
              Thread.Sleep(200);
          }
        }
      }
    }

    public App()
    {
      try
      {
        LogUtils.WriteLog(LogLevel.Debug, "DI 컨테이너 구성 시작");
        Services = ConfigureServices();
        LogUtils.WriteLog(LogLevel.Debug, "DI 컨테이너 구성 완료");
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "DI 컨테이너 구성 실패", ex);
        throw;
      }
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      LogUtils.WriteLog(LogLevel.Info, "Application_Startup 진입");
      try
      {
        //var testView = new TestApp();
        //testView.Show();
        //return;

        LogUtils.WriteLog(LogLevel.Debug, "AutoStartService 처리");
        var autoStartService = Services.GetRequiredService<IAutoStartService>();
        autoStartService.MigrateIfNeeded();
        autoStartService.DeleteStartup(); // 자동 실행 삭제

        // 설정 로드
        LogUtils.WriteLog(LogLevel.Debug, "SettingsService 로드");
        var settingsService = Services.GetRequiredService<ISettingsService>();
        settingsService.UpdateSettings(isBootPopupBrowserEnabled: false);
        settingsService.LoadSettings();

        LogUtils.WriteLog(LogLevel.Debug, "MainView 표시");
        var viewService = Services.GetRequiredService<IViewService>();
        viewService.ShowMainView();

        _heartbeat = new HeartbeatLogger(TimeSpan.FromSeconds(60));
        _heartbeat.Start();

        LogUtils.WriteLog(LogLevel.Info, "Application_Startup 완료");
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "Application_Startup 실패", ex);
        throw;
      }
    }

    private void OnUpdateError(object? sender, UpdateErrorEventArgs e)
    {
      // 오류 발생 시 에러 로그 추가 (기존 동작 유지)
      LogUtils.WriteLog(LogLevel.Error, $"업데이트 오류: {e.Message}");
    }

    protected override void OnExit(ExitEventArgs e)
    {
      _heartbeat?.Dispose();
      _heartbeat = null;
      base.OnExit(e);
    }

    /// <summary>
    /// 실행 중인 프로세스의 버전을 비교하고 처리합니다.
    /// </summary>
    private static void HandleExistingProcess()
    {
      try
      {
        var currentVersion = GetCurrentVersion();
        var existingProcess = GetExistingProcess();

        if (existingProcess == null)
        {
          Msg.Show("이미 실행 중입니다.");
          return;
        }

        var existingVersion = GetProcessVersion(existingProcess);

        if (currentVersion > existingVersion)
        {
          // 새 버전이 더 높으면 기존 프로세스 종료 후 현재 프로세스 실행
          LogUtils.WriteLog(LogLevel.Info, $"새로운 버전 감지 ({existingVersion} -> {currentVersion}). 기존 프로세스를 종료합니다.");

          try
          {
            existingProcess.Kill();
            existingProcess.WaitForExit(3000);
          }
          catch (Exception ex)
          {
            LogUtils.WriteLog(LogLevel.Error, $"기존 프로세스 종료 실패: {ex.Message}");
          }

          // Mutex를 다시 시도 (기존 프로세스가 종료되었으므로 획득 가능)
          if (TryAcquireMutex())
            RunApp();
        }
        else
        {
          LogUtils.WriteLog(LogLevel.Info, $"현재 버전이 낮거나 같음 ({currentVersion} <= {existingVersion}). 프로세스를 종료합니다.");
        }
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, $"프로세스 버전 비교 중 오류: {ex.Message}");
      }
    }

    /// <summary>
    /// 현재 실행 중인 애플리케이션의 버전을 가져옵니다.
    /// </summary>
    private static Version GetCurrentVersion()
    {
      var version = Assembly.GetExecutingAssembly().GetName().Version;
      return version ?? new Version("0.0.0.0");
    }

    /// <summary>
    /// 실행 중인 같은 이름의 다른 프로세스를 찾습니다.
    /// </summary>
    private static Process? GetExistingProcess()
    {
      var currentProcess = Process.GetCurrentProcess();
      var currentProcessName = Path.GetFileNameWithoutExtension(currentProcess.MainModule?.FileName ?? "");

      var processes = Process.GetProcessesByName(currentProcessName);

      // 현재 프로세스 자신을 제외하고 다른 프로세스 찾기
      foreach (var process in processes)
      {
        if (process.Id != currentProcess.Id)
        {
          return process;
        }
      }

      return null;
    }

    /// <summary>
    /// 프로세스의 버전 정보를 가져옵니다.
    /// </summary>
    private static Version GetProcessVersion(Process process)
    {
      try
      {
        var versionInfo = process.MainModule?.FileVersionInfo;
        if (versionInfo != null)
        {
          var versionString = versionInfo.FileVersion;
          if (Version.TryParse(versionString, out var version))
          {
            return version;
          }
        }
      }
      catch
      {
        // 버전 정보를 가져올 수 없으면 0.0.0.0으로 반환
      }

      return new Version("0.0.0.0");
    }

    /// <summary>
    /// Mutex 획득을 시도합니다. 버려진 Mutex는 무시하고 새로 생성합니다.
    /// </summary>
    private static bool TryAcquireMutex()
    {
      try
      {
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (createdNew)
        {
          LogUtils.WriteLog(LogLevel.Debug, "Mutex 새로 생성됨");
          return true;
        }

        // Mutex가 이미 존재하는 경우 획득 시도
        bool acquired = _mutex.WaitOne(TimeSpan.Zero, true);
        LogUtils.WriteLog(LogLevel.Debug, $"Mutex 기존 존재, 획득 시도 결과={acquired}");
        return acquired;
      }
      catch (AbandonedMutexException)
      {
        LogUtils.WriteLog(LogLevel.Warn, "버려진 Mutex 감지 — 새로 생성 (이전 프로세스 비정상 종료 추정)");
        _mutex?.Dispose();
        _mutex = new Mutex(true, MutexName, out _);
        return true;
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "Mutex 획득 중 예외", ex);
        return false;
      }
    }
  }
}
