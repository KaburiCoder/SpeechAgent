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
    void ShowSettingsView(Window parent);
    void ShowFindWinView(Window parent);
    void ShowFindWinApiView(Window parent);
    void ShowFindWinImageView(Window parent);
    void ShowUpdateHistoryView(Window parent);
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

    private void ShowDialogCommon<TView, TViewModel>(Window parent)
      where TView : Window, new()
      where TViewModel : BaseViewModel
    {
      var viewModelFactory = App.Current.Services.GetRequiredService<IViewModelFactory>();
      var result = viewModelFactory.CreateViewModel<TView, TViewModel>(parent);

      result.View.ShowDialog();
    }

    public void ShowSettingsView(Window parent)
    {
      ShowDialogCommon<SettingsView, SettingsViewModel>(parent);
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
