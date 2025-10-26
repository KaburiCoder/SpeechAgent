using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Features.Settings.FindWin.Models;
using SpeechAgent.Features.Settings.FindWin.Services;
using SpeechAgent.Messages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace SpeechAgent.Features.Settings.FindWin
{
  partial class FindWinImageViewModel : BaseViewModel
  {
    private readonly WindowCaptureService _captureService;

    [ObservableProperty]
    private ObservableCollection<WindowInfo> _windows = new();

    [ObservableProperty]
    private WindowInfo? _selectedWindow;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private BitmapSource? _selectedWindowImage;

    [ObservableProperty]
    private string _customImageName = string.Empty;

    [ObservableProperty]
    private string _customImageRect = string.Empty;

    [ObservableProperty]
    private string _rectX = string.Empty;

    [ObservableProperty]
    private string _rectY = string.Empty;

    [ObservableProperty]
    private string _rectWidth = string.Empty;

    [ObservableProperty]
    private string _rectHeight = string.Empty;

    [ObservableProperty]
    private BitmapSource? _croppedImage;

    public FindWinImageViewModel()
    {
      _captureService = new WindowCaptureService();
    }

    [RelayCommand]
    private async Task StartScan()
    {
      IsLoading = true;
      Windows.Clear();
      SelectedWindow = null;
      SelectedWindowImage = null;
      CroppedImage = null;

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

    [RelayCommand]
    private void UpdateCroppedImage()
    {
      if (SelectedWindowImage == null) return;

      if (!int.TryParse(RectX, out int x) || !int.TryParse(RectY, out int y) ||
      !int.TryParse(RectWidth, out int width) || !int.TryParse(RectHeight, out int height))
      {
        CroppedImage = null;
        CustomImageRect = string.Empty;
        return;
      }

      if (x < 0 || y < 0 || width <= 0 || height <= 0 ||
          x + width > SelectedWindowImage.PixelWidth ||
        y + height > SelectedWindowImage.PixelHeight)
      {
        CroppedImage = null;
        CustomImageRect = string.Empty;
        return;
      }

      try
      {
        var croppedBitmap = new CroppedBitmap(SelectedWindowImage, new System.Windows.Int32Rect(x, y, width, height));
        CroppedImage = croppedBitmap;
        CustomImageRect = $"{x},{y},{width},{height}";
      }
      catch (Exception ex)
      {
        MessageBox.Show($"이미지 자르기 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        CroppedImage = null;
        CustomImageRect = string.Empty;
      }
    }

    [RelayCommand]
    private void SendToSettings()
    {
      if (string.IsNullOrEmpty(CustomImageName))
      {
        MessageBox.Show("이름을 입력해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      if (string.IsNullOrEmpty(CustomImageRect))
      {
        MessageBox.Show("X, Y, Width, Height 값을 입력해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      WeakReferenceMessenger.Default.Send(new SendToSettingsImageMessage(CustomImageName, CustomImageRect));
      View.Close();
    }

    partial void OnSelectedWindowChanged(WindowInfo? value)
    {
      if (value != null)
      {
        SelectedWindowImage = value.Screenshot;
        CustomImageName = value.Title;
        CroppedImage = null;
        RectX = string.Empty;
        RectY = string.Empty;
        RectWidth = string.Empty;
        RectHeight = string.Empty;
        CustomImageRect = string.Empty;
      }
    }

    partial void OnRectXChanged(string value) => UpdateCroppedImage();
    partial void OnRectYChanged(string value) => UpdateCroppedImage();
    partial void OnRectWidthChanged(string value) => UpdateCroppedImage();
    partial void OnRectHeightChanged(string value) => UpdateCroppedImage();
  }
}
