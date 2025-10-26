using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Constants;
using SpeechAgent.Database;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using SpeechAgent.Features.Settings.FindWin;
using SpeechAgent.Features.Settings.FindWin.Services;
using SpeechAgent.Services;
using SpeechAgent.Services.Api;
using SpeechAgent.Services.MedicSIO;
using SpeechAgent.Utils;
using SpeechAgent.Utils.Automation;
using System.Net.Http;
using System.Windows;
using Velopack;

namespace SpeechAgent
{
  public partial class App : System.Windows.Application
  {
    public new static App Current => (App)System.Windows.Application.Current;
    public IServiceProvider Services { get; } = default!;

    private static IServiceProvider ConfigureServices()
    {
      var services = new ServiceCollection();

      // Singletons
      services.AddHttpClient("SpeechServer", client =>
      {
        var settingsService = Current.Services.GetRequiredService<ISettingsService>();

        client.BaseAddress = new Uri(ApiConfig.SpeechBaseUrl);
        client.DefaultRequestHeaders.Add(ApiConfig.SpeechUserKey, settingsService.Settings.ConnectKey);
      });
      services.AddSingleton<HttpClient>();
      services.AddSingleton<IViewService, ViewService>();
      services.AddSingleton<IViewModelFactory, ViewModelFactory>();
      services.AddSingleton<IPatientSearchService, PatientSearchService>();
      services.AddSingleton<ISettingsService, SettingsService>();
      services.AddSingleton<IMedicSIOService, MedicSIOService>();
      services.AddSingleton<TrayIconService>();
      services.AddSingleton<IUpdateService, UpdateService>();
      services.AddSingleton<IAutoStartService, AutoStartService>();

      // Views
      services.AddSingleton<MainView>();

      // ViewModels
      services.AddTransient<MainViewModel>();
      services.AddTransient<SettingsViewModel>();
      services.AddTransient<FindWinViewModel>();
      services.AddTransient<FindWinImageViewModel>();

      // Services
      services.AddTransient<IMainService, MainService>();
      services.AddTransient<IWindowCaptureService, WindowCaptureService>();
      services.AddTransient<IAutomationControlSearcher, AutomationControlSearcher>();

      services.AddTransient<ILlmApi, LlmApi>();
      return services.BuildServiceProvider();
    }

    static Mutex _mutex = new(true, "SpeechAgent_UniqueMutexName");

    [STAThread]
    public static void Main()
    {
      if (_mutex.WaitOne(TimeSpan.Zero, true) == false)
      {
        Msg.Show("이미 실행 중입니다.");
        return;
      }

      using (var context = new AppDbContext())
      {
        context.Database.Migrate();
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

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
      // UpdateService 시작
      var updateService = Services.GetRequiredService<IUpdateService>();
      await updateService.CheckForUpdatesAsync();
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
