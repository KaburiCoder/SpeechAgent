using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using SpeechAgent.Services;
using SpeechAgent.Services.MedicSIO;
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

    public App()
    {
      Services = ConfigureServices();
    }       

    [STAThread]
    public static void Main()
    {
      // Velopack을 진입점에서 가장 먼저 실행
      VelopackApp.Build().Run();

      // WPF Application 실행
      var app = new App();
      app.InitializeComponent();
      app.Run();
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
