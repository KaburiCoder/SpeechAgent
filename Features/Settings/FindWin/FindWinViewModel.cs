using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Features.Settings.FindWin.Models;
using SpeechAgent.Features.Settings.FindWin.Services;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Utils.Automation;
using System.Collections.ObjectModel;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace SpeechAgent.Features.Settings.FindWin
{
  partial class FindWinViewModel : BaseViewModel
  {
    private readonly WindowCaptureService _captureService;
    private readonly AutomationControlSearcher _automationSearcher;

    [ObservableProperty]
    private ObservableCollection<WindowInfo> _windows = new();

    [ObservableProperty]
    private WindowInfo? _selectedWindow;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<AutomationControlInfo> _searchedControls = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _chartNumberControlType = string.Empty;

    [ObservableProperty]
    private string _chartNumberIndex = string.Empty;

    [ObservableProperty]
    private string _patientNameControlType = string.Empty;

    [ObservableProperty]
    private string _patientNameIndex = string.Empty;

    public FindWinViewModel()
    {
      _captureService = new WindowCaptureService();
      _automationSearcher = new AutomationControlSearcher();
    }

    [RelayCommand]
    private async Task StartScan()
    {
      IsLoading = true;
      Windows.Clear();
      SelectedWindow = null;
      SearchedControls.Clear();

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
        MessageBox.Show($"��ĵ �� ������ �߻��߽��ϴ�: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        IsLoading = false;
      }
    }

    [RelayCommand]
    internal void Search()
    {
      if (string.IsNullOrWhiteSpace(SearchText))
      {
        // �˻�� ������ ��� ��Ʈ�� ǥ��
        SearchedControls.Clear();
         
        return;
      }

      var filtered = _automationSearcher.FoundControls
        .Where(c => c.Text.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                   c.ClassName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        .ToList();

      SearchedControls.Clear();
      foreach (var control in filtered)
      {
        SearchedControls.Add(control);
      }
    }

    [RelayCommand]
    private void SendToSettings()
    {
      if (string.IsNullOrEmpty(ChartNumberControlType) || string.IsNullOrEmpty(ChartNumberIndex))
      {
        MessageBox.Show("��Ʈ��ȣ�� ��Ʈ�� Ÿ�԰� �ε����� �Է����ּ���.", "�˸�", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      if (string.IsNullOrEmpty(PatientNameControlType) || string.IsNullOrEmpty(PatientNameIndex))
      {
        MessageBox.Show("�����ڸ��� ��Ʈ�� Ÿ�԰� �ε����� �Է����ּ���.", "�˸�", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      WeakReferenceMessenger.Default.Send(new SendToSettingsMessage(
        exeTitle: SelectedWindow?.Title ?? "",
        chartControlType: ChartNumberControlType,
        chartIndex: ChartNumberIndex,
        nameControlType: PatientNameControlType,
        nameIndex: PatientNameIndex
      ));

      View.Close();
    }

    partial void OnSelectedWindowChanged(WindowInfo? value)
    {
      SearchedControls.Clear();

      if (value != null)
      {
        IsLoading = true;

        Task.Run(() =>
        {
          try
          {
            // UI Automation���� ��Ʈ�� �˻�
            if (_automationSearcher.FindWindowByTitle(title => title.Contains(value.Title)))
            {
              var controls = _automationSearcher.SearchControls();

              App.Current.Dispatcher.Invoke(() =>
              {
                SearchedControls.Clear();
                foreach (var control in controls)
                {
                  SearchedControls.Add(control);
                }
              });
            }
          }
          catch (Exception ex)
          {
            App.Current.Dispatcher.Invoke(() =>
            {
              MessageBox.Show($"��Ʈ�� �˻� �� ������ �߻��߽��ϴ�: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            });
          }
          finally
          {
            App.Current.Dispatcher.Invoke(() =>
            {
              IsLoading = false;
            });
          }
        });
      }
    }

    [RelayCommand]
    private void AssignToChart(AutomationControlInfo controlInfo)
    {
      if (controlInfo != null)
      {
        ChartNumberControlType = controlInfo.ControlType;
        ChartNumberIndex = controlInfo.Index.ToString();
      }
    }

    [RelayCommand]
    private void AssignToName(AutomationControlInfo controlInfo)
    {
      if (controlInfo != null)
      {
        PatientNameControlType = controlInfo.ControlType;
        PatientNameIndex = controlInfo.Index.ToString();
      }
    }
  }
}
