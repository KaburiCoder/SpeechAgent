using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Bases;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using SpeechAgent.Features.Settings.FindWin;
using SpeechAgent.Features.UpdateHistory;

namespace SpeechAgent.Services
{
  public interface IViewService
  {
    void ShowMainView();
    void ShowSettingsView(Window? parent = null);
    void ShowFindWinView(Window parent);
    void ShowFindWinApiView(Window parent);
    void ShowFindWinImageView(Window parent);
    void ShowUpdateHistoryView(Window parent);
  }

  public class ViewService : IViewService
  {
    private SettingsView? _settingsViewInstance;

    public void ShowMainView()
    {
      var mainView = App.Current.Services.GetRequiredService<MainView>();
      var mainViewModel = App.Current.Services.GetRequiredService<MainViewModel>();

      mainViewModel.SetView(mainView);
      mainViewModel.Initialize();
      mainView.DataContext = mainViewModel;

      mainView.Show();
    }

    private void ShowDialogCommon<TView, TViewModel>(Window? parent)
      where TView : Window, new()
      where TViewModel : BaseViewModel
    {
      var viewModelFactory = App.Current.Services.GetRequiredService<IViewModelFactory>();
      var result = viewModelFactory.CreateViewModel<TView, TViewModel>(parent);

      result.View.ShowDialog();
    }

    public void ShowSettingsView(Window? parent)
    {
      // 이미 열려있는 SettingsView가 있으면 활성화만 함
      if (_settingsViewInstance != null && _settingsViewInstance.IsVisible)
      {
        _settingsViewInstance.Activate();
        _settingsViewInstance.Focus();
        return;
      }

      var viewModelFactory = App.Current.Services.GetRequiredService<IViewModelFactory>();
      var result = viewModelFactory.CreateViewModel<SettingsView, SettingsViewModel>(parent);

      _settingsViewInstance = result.View;

      // 윈도우가 닫혔을 때 인스턴스 제거
      _settingsViewInstance.Closed += (s, e) =>
      {
        _settingsViewInstance = null;
      };

      _settingsViewInstance.ShowDialog();
    }

    public void ShowFindWinView(Window parent)
    {
      ShowDialogCommon<FindWinView, FindWinViewModel>(parent);
    }

    public void ShowFindWinApiView(Window parent)
    {
      ShowDialogCommon<FindWinApiView, FindWinApiViewModel>(parent);
    }

    public void ShowFindWinImageView(Window parent)
    {
      ShowDialogCommon<FindWinImageView, FindWinImageViewModel>(parent);
    }

    public void ShowUpdateHistoryView(Window parent)
    {
      ShowDialogCommon<UpdateHistoryView, UpdateHistoryViewModel>(parent);
    }
  }
}
