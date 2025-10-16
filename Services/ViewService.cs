using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;

namespace SpeechAgent.Services
{
  public interface IViewService
  {
    void ShowMainView();
    void ShowSettingsView();
  }

  public class ViewService : IViewService
  {
    public void ShowMainView()
    {
      var viewModelFactory = App.Current.Services.GetRequiredService<IViewModelFactory>();
      var main = viewModelFactory.CreateViewModel<MainView, MainViewModel>();

      App.Current.MainView = main.View;
      main.View.ShowDialog();
    }

    public void ShowSettingsView()
    {
      var viewModelFactory = App.Current.Services.GetRequiredService<IViewModelFactory>();
      var settings = viewModelFactory.CreateViewModel<SettingsView, SettingsViewModel>(App.Current.MainView);

      settings.View.ShowDialog();
    }
  }
}
