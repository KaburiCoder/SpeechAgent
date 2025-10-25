using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Features.Settings.FindWin.Models;
using SpeechAgent.Features.Settings.FindWin.Services;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace SpeechAgent.Features.Settings.FindWin
{
  partial class FindWinViewModel : BaseViewModel
  {
    private readonly WindowCaptureService _captureService;
    private ControlSearcher _searcher = new();

    [ObservableProperty]
    private ObservableCollection<WindowInfo> _windows = new();

    [ObservableProperty]
    private ObservableCollection<ControlInfo> _controls = new();

    [ObservableProperty]
    private ObservableCollection<ControlInfo> _searchedControls = new();

    [ObservableProperty]
    private WindowInfo? _selectedWindow;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    private bool _isChartNumberSelected = true;
    public bool IsChartNumberSelected
    {
      get => _isChartNumberSelected;
      set
      {
        if (SetProperty(ref _isChartNumberSelected, value) && value)
        {
          IsPatientNameSelected = false;
        }
      }
    }

    private bool _isPatientNameSelected;
    public bool IsPatientNameSelected
    {
      get => _isPatientNameSelected;
      set
      {
        if (SetProperty(ref _isPatientNameSelected, value) && value)
        {
          IsChartNumberSelected = false;
        }
      }
    }

    private string _chartNumberClassName = string.Empty;
    public string ChartNumberClassName
    {
      get => _chartNumberClassName;
      set => SetProperty(ref _chartNumberClassName, value);
    }

    private string _chartNumberIndex = string.Empty;
    public string ChartNumberIndex
    {
      get => _chartNumberIndex;
      set => SetProperty(ref _chartNumberIndex, value);
    }

    private string _patientNameClassName = string.Empty;
    public string PatientNameClassName
    {
      get => _patientNameClassName;
      set => SetProperty(ref _patientNameClassName, value);
    }

    private string _patientNameIndex = string.Empty;
    public string PatientNameIndex
    {
      get => _patientNameIndex;
      set => SetProperty(ref _patientNameIndex, value);
    }

    private ControlInfo? _selectedControlInfo;
    public ControlInfo? SelectedControlInfo
    {
      get => _selectedControlInfo;
      set
      {
        if (SetProperty(ref _selectedControlInfo, value) && value != null)
        {
          if (IsChartNumberSelected)
          {
            ChartNumberClassName = value.ClassName;
            ChartNumberIndex = value.Index.ToString();
          }
          else if (IsPatientNameSelected)
          {
            PatientNameClassName = value.ClassName;
            PatientNameIndex = value.Index.ToString();
          }
        }
      }
    }


    public FindWinViewModel()
    {
      _captureService = new WindowCaptureService();
    }

    [RelayCommand]
    private async Task StartScan()
    {
      IsLoading = true;
      Windows.Clear();
      Controls.Clear();
      SelectedWindow = null;
      UpdateSearchedControls();

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

    private async void LoadDetailedInfo(WindowInfo windowInfo)
    {
      try
      {
        await Task.Run(() =>
        {
          // 선택된 윈도우의 모든 자식 컨트롤 검색
          _searcher.SetHwnd(windowInfo.Handle);
          List<ControlInfo> controls = _searcher.SearchControls();

          // 찾은 컨트롤들을 Controls 컬렉션에 추가
          App.Current.Dispatcher.Invoke(() =>
          {
            Controls.Clear();
            for (int i = 0; i < controls.Count; i++)
            {
              controls[i].Index = i;
              Controls.Add(controls[i]);
            }

            // 클래스별로 그룹화하여 Index 재설정
            var grouped = Controls.GroupBy(c => c.ClassName);
            foreach (var group in grouped)
            {
              int index = 0;
              foreach (var control in group)
              {
                control.Index = index++;
              }
            }

            UpdateSearchedControls();
          });
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($"세부 정보 로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void UpdateSearchedControls()
    {
      var searched = string.IsNullOrWhiteSpace(SearchText) 
        ? Controls 
        : Controls.Where(c => c.Text.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || c.ClassName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
      SearchedControls = new ObservableCollection<ControlInfo>(searched);
    }

    [RelayCommand]
    internal void Search()
    {
      UpdateSearchedControls();
    }

    [RelayCommand]
    private void SendToSettings()
    {
      WeakReferenceMessenger.Default.Send(new SendToSettingsMessage(SelectedWindow?.Title ?? "", ChartNumberClassName, ChartNumberIndex, PatientNameClassName, PatientNameIndex));

      View.Close();
    }

    partial void OnSelectedWindowChanged(WindowInfo? value)
    {
      if (value != null)
      {
        LoadDetailedInfo(value);
      }
    }

    partial void OnSearchTextChanged(string value)
    {
      // 실시간 검색 제거
    }
  }
}
