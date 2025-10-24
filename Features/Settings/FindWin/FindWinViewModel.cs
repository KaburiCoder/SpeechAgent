using CommunityToolkit.Mvvm.Input;
using SpeechAgent.Bases;
using SpeechAgent.Features.Settings.FindWin.Models;
using SpeechAgent.Features.Settings.FindWin.Services;
using System.Collections.ObjectModel;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace SpeechAgent.Features.Settings.FindWin
{
  internal class FindWinViewModel : BaseViewModel
  {
    private readonly WindowCaptureService _captureService;
    private WindowInfo? _selectedWindow;
    private WindowInfo? _detailedWindowInfo;
    private bool _isLoading;

    public ObservableCollection<WindowInfo> Windows { get; } = new();

    public WindowInfo? SelectedWindow
    {
      get => _selectedWindow;
      set
      {
        if (SetProperty(ref _selectedWindow, value) && value != null)
        {
          LoadDetailedInfo(value);
        }
      }
    }

    public WindowInfo? DetailedWindowInfo
    {
      get => _detailedWindowInfo;
      set => SetProperty(ref _detailedWindowInfo, value);
    }

    public bool IsLoading
    {
      get => _isLoading;
      set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand StartScanCommand { get; }
    public IRelayCommand RefreshCommand { get; }

    public FindWinViewModel()
    {
      _captureService = new WindowCaptureService();
      StartScanCommand = new RelayCommand(StartScan);
      RefreshCommand = new RelayCommand(RefreshWindows);
    }

    private async void StartScan()
    {
      IsLoading = true;
      Windows.Clear();
      DetailedWindowInfo = null;
      SelectedWindow = null;

      try
      {
        await Task.Run(() =>
        {
          var windows = _captureService.GetWindowsWithScreenshots();

          App.Current.Dispatcher.Invoke(() =>
                  {
                    foreach (var window in windows)
                    {
                      Windows.Add(window);
                    }
                  });
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($"스캔 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        IsLoading = false;
      }
    }

    private async void RefreshWindows()
    {
      StartScan();
    }

    private async void LoadDetailedInfo(WindowInfo windowInfo)
    {
      try
      {
        await Task.Run(() =>
        {
          var detailedInfo = _captureService.GetDetailedWindowInfo(windowInfo.Handle);

          App.Current.Dispatcher.Invoke(() =>
                  {
                    DetailedWindowInfo = detailedInfo;
                  });
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($"세부 정보 로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }
}
