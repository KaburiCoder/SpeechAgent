using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using SpeechAgent.Services;
using SpeechAgent.Services.MedicSIO;
using System.Windows;

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

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      var viewService = Services.GetRequiredService<IViewService>();
      viewService.ShowMainView();
    }
  }
}
