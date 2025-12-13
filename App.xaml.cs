using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
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
using SpeechAgent.Services.Globals;
using SpeechAgent.Services.MedicSIO;
using SpeechAgent.Utils;
using SpeechAgent.Utils.Automation;
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
      services.AddSingleton<IMedicSIOService, MedicSIOService>();
      services.AddSingleton<TrayIconService>();
      services.AddSingleton<IUpdateService, UpdateService>();
      services.AddSingleton<IAutoStartService, AutoStartService>();
      services.AddSingleton<IUserNotificationService, UserNotificationService>();

      services.AddSingleton<IGlobalKeyHook, GlobalKeyHook>();

      // Views
      services.AddSingleton<MainView>();

      // ViewModels
      services.AddTransient<MainViewModel>();
      services.AddTransient<SettingsViewModel>();
      services.AddTransient<FindWinViewModel>();
      services.AddTransient<FindWinApiViewModel>();
      services.AddTransient<FindWinImageViewModel>();
      services.AddTransient<ShortcutSettingsViewModel>();
      services.AddTransient<UpdateHistoryViewModel>();

      // Services
      services.AddTransient<IMainService, MainService>();
      services.AddTransient<IWindowCaptureService, WindowCaptureService>();
      services.AddTransient<IAutomationControlSearcher, AutomationControlSearcher>();
      services.AddTransient<IControlSearcher, ControlSearcher>();
      services.AddTransient<IClickSoftControlSearchService, ClickSoftControlSearchService>();
      services.AddTransient<IShortcutSettingsService, ShortcutSettingsService>();
      services.AddTransient<IUpdateHistoryService, UpdateHistoryService>();

      services.AddTransient<ILlmApi, LlmApi>();
      services.AddTransient<IUserNotificationsApi, UserNotificationsApi>();

      return services.BuildServiceProvider();
    }

    static Mutex _mutex = new(true, "SpeechAgent_UniqueMutexName");

    [STAThread]
    public static void Main()
    {
      // 작업 디렉토리를 실행 파일 위치로 변경(윈도우 재시작 후 바로가기로 실행 시 문제 방지)
      Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

      // 관리자 권한 확인 및 필요시 재실행
      AdminHelper.RequireAdminOrExit();
      // EUC-KR, CP949, 949
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

      if (_mutex.WaitOne(TimeSpan.Zero, true) == false)
      {
        Msg.Show("이미 실행 중입니다.");
        return;
      }

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

      // Velopack을 진입점에서 가장 먼저 실행
      VelopackApp.Build().Run();

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

      // Shortcut Migrate
      var autoStartService = Services.GetRequiredService<IAutoStartService>();
      autoStartService.MigrateToShortcutIfNeeded();

      // UpdateService 시작
      var updateService = Services.GetRequiredService<IUpdateService>();
      updateService.UpdateError += OnUpdateError;
      updateService.StartPeriodicCheck();

      // 설정 로드
      var settingsService = Services.GetRequiredService<ISettingsService>();
      settingsService.LoadSettings();

      var viewService = Services.GetRequiredService<IViewService>();
      viewService.ShowMainView();
    }

    private void OnUpdateError(object? sender, UpdateErrorEventArgs e)
    {
      // 오류 발생 시 메시지박스 표시 (기존 동작 유지)
      System.Windows.MessageBox.Show($"업데이트 오류: {e.Message}");
    }
  }
}
