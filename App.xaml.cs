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

    private static async Task UpdateMyApp()
    {
#if !DEBUG
      var mgr = new UpdateManager("https://github.com/KaburiCoder/SpeechAgent/releases/latest/download");

      // check for new version
      try
      {
        var newVersion = await mgr.CheckForUpdatesAsync();        
        if (newVersion == null)
          return; // no update available
                  // download new version
        await mgr.DownloadUpdatesAsync(newVersion);

        // install new version and restart app
        mgr.ApplyUpdatesAndRestart(newVersion);
      }
      catch (Exception err)
      {
        System.Windows.MessageBox.Show(err.ToString());
      }
#else
      await Task.CompletedTask;
#endif
    }

    [STAThread]
    public static void Main()
    {
      // Velopack을 진입점에서 가장 먼저 실행
      VelopackApp.Build().Run();
      _ = UpdateMyApp();

      // WPF Application 실행
      var app = new App();
      app.InitializeComponent();
      app.Run();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      var viewService = Services.GetRequiredService<IViewService>();
      viewService.ShowMainView();
    }
  }
}
