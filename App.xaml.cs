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

    [STAThread]
    public static void Main()
    {
      KillLegacyProcesses();
      // 작업 디렉토리를 실행 파일 위치로 변경(윈도우 재시작 후 바로가기로 실행 시 문제 방지)
      Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

      // 관리자 권한 확인 및 필요시 재실행
      AdminHelper.RequireAdminOrExit();
      // EUC-KR, CP949, 949
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

      // 실행 중인 프로세스 버전 비교 및 처리
      if (!TryAcquireMutex())
      {
        HandleExistingProcess();
        return;
      }

      RunApp();
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

    private static void RunApp()
    {
      // 데이터베이스 마이그레이션 적용
      using (var context = new AppDbContext())
      {
        string dbPath = context.DbPath;
        try
        {
          context.Database.Migrate();
        }
        catch (SqliteException ex)
        {
          if (ex.SqliteErrorCode == 1)
          {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
          }
        }
      }

      // WPF Application 실행
      var app = new App();
      app.InitializeComponent();
      app.Run();
    }

    public App()
    {
      Services = ConfigureServices();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      //var testView = new TestApp();
      //testView.Show();
      //return;

      var autoStartService = Services.GetRequiredService<IAutoStartService>();
      autoStartService.MigrateIfNeeded();
      autoStartService.DeleteStartup(); // 자동 실행 삭제

      // 설정 로드
      var settingsService = Services.GetRequiredService<ISettingsService>();
      settingsService.UpdateSettings(isBootPopupBrowserEnabled: false);
      settingsService.LoadSettings();

      var viewService = Services.GetRequiredService<IViewService>();
      viewService.ShowMainView();
    }

    private void OnUpdateError(object? sender, UpdateErrorEventArgs e)
    {
      // 오류 발생 시 에러 로그 추가 (기존 동작 유지)
      LogUtils.WriteLog(LogLevel.Error, $"업데이트 오류: {e.Message}");
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
          return true;
        }

        // Mutex가 이미 존재하는 경우 획득 시도
        return _mutex.WaitOne(TimeSpan.Zero, true);
      }
      catch (AbandonedMutexException)
      {
        // 버려진 Mutex인 경우 새로 생성 (이전 프로세스가 비정상 종료됨)
        _mutex?.Dispose();
        _mutex = new Mutex(true, MutexName, out _);
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
