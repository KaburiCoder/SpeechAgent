using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using SpeechAgent.Features.Settings.FindWin;
using System.Windows;

namespace SpeechAgent.Services
{
  public interface IViewService
  {
    void ShowMainView();
    void ShowSettingsView(Window parent);
    void ShowFindWinView(Window parent);
  }

  public class ViewService : IViewService
  {

    public void ShowMainView()
    {
      var mainView = App.Current.Services.GetRequiredService<MainView>();
      var mainViewModel = App.Current.Services.GetRequiredService<MainViewModel>();

      mainViewModel.SetView(mainView);
      mainViewModel.Initialize();
      mainView.DataContext = mainViewModel;

      mainView.Show();
    }

    public void ShowSettingsView(Window parent)
    {
      var viewModelFactory = App.Current.Services.GetRequiredService<IViewModelFactory>();
      var settings = viewModelFactory.CreateViewModel<SettingsView, SettingsViewModel>(parent);

      settings.View.ShowDialog();
    }

    public void ShowFindWinView(Window parent)
    {
      var viewModelFactory = App.Current.Services.GetRequiredService<IViewModelFactory>();
      var findWin = viewModelFactory.CreateViewModel<FindWinView, FindWinViewModel>(parent);
      findWin.View.ShowDialog();
    }
  }
}
