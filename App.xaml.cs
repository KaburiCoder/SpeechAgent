using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using SpeechAgent.Services;
using SpeechAgent.Services.MedicSIO;
using System.Globalization;
using System.Windows;
using Velopack;
using Velopack.Sources;

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
      services.AddSingleton<IViewService, ViewService>();
      services.AddSingleton<IViewModelFactory, ViewModelFactory>();
      services.AddSingleton<IControlSearchService, ControlSearchService>();
      services.AddSingleton<ISettingsService, SettingsService>();
      services.AddSingleton<IMedicSIOService, MedicSIOService>();
      services.AddSingleton<TrayIconService>();
      services.AddSingleton<IUpdateService, UpdateService>();

      // Views
      services.AddSingleton<MainView>();

      // ViewModels
      services.AddTransient<MainViewModel>();
      services.AddTransient<SettingsViewModel>();

      // Services
      services.AddTransient<IMainService, MainService>();

      return services.BuildServiceProvider();
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern bool SetConsoleOutputCP(uint wCodePageID);

    [STAThread]
    public static void Main()
    {
      // UTF-8 코드 페이지 설정 (CP65001)
      SetConsoleOutputCP(65001);

      // 한국어 로케일 설정
      CultureInfo koKr = new("ko-KR");
      Thread.CurrentThread.CurrentCulture = koKr;
      Thread.CurrentThread.CurrentUICulture = koKr;

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
