using SpeechAgent.Features.Settings.FindWin.Models;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Vanara.PInvoke;

namespace SpeechAgent.Features.Settings.FindWin.Services
{
  internal class WindowCaptureService
  {
    public List<WindowInfo> GetAllWindows()
    {
      var windows = new List<WindowInfo>();

      User32.EnumWindows((hWnd, lParam) =>
      {
        if (!User32.IsWindowVisible(hWnd) || User32.IsIconic(hWnd))
          return true;

        int length = User32.GetWindowTextLength(hWnd);
        if (length == 0)
          return true;

        var title = new StringBuilder(length + 1);
        User32.GetWindowText(hWnd, title, title.Capacity);

        var className = new StringBuilder(256);
        User32.GetClassName(hWnd, className, className.Capacity);

        User32.GetWindowThreadProcessId(hWnd, out uint processId);
        string processName = string.Empty;

        try
        {
          var process = Process.GetProcessById((int)processId);
          processName = process.ProcessName;
        }
        catch { }

        User32.GetWindowRect(hWnd, out var rect);

        var windowInfo = new WindowInfo
        {
          Handle = (IntPtr)hWnd.DangerousGetHandle(),
          Title = title.ToString(),
          ClassName = className.ToString(),
          ProcessName = processName,
          ProcessId = (int)processId,
          IsVisible = true,
          Width = rect.Width,
          Height = rect.Height
        };

        windows.Add(windowInfo);
        return true;
      }, IntPtr.Zero);

      return windows;
    }

    public BitmapSource? CaptureWindow(IntPtr hWnd)
    {
      try
      {
        if (!User32.IsWindow(hWnd))
          return null;

        // DWM을 사용하여 정확한 윈도우 크기 가져오기 (그림자 제외)
        RECT rect;
        var result = DwmApi.DwmGetWindowAttribute(new HWND(hWnd), DwmApi.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out rect);

        // DWM이 실패하면 일반 GetWindowRect 사용
        if (result.Failed)
        {
          User32.GetWindowRect(hWnd, out rect);
        }

        int width = rect.Width;
        int height = rect.Height;

        if (width <= 0 || height <= 0)
          return null;

        // 비트맵 생성
        using (Bitmap bitmap = new Bitmap(width, height))
        {
          using (Graphics graphics = Graphics.FromImage(bitmap))
          {
            var hdc = graphics.GetHdc();
            Gdi32.SafeHDC safeHdc = new Gdi32.SafeHDC(hdc, false);

            // PrintWindow를 사용하여 윈도우 내용 캡처
            User32.PrintWindow(new HWND(hWnd), safeHdc, User32.PW.PW_RENDERFULLCONTENT);

            graphics.ReleaseHdc(hdc);
          }

          // Bitmap을 BitmapSource로 변환
          return BitmapToBitmapSource(bitmap);
        }
      }
      catch
      {
        return null;
      }
    }

    private BitmapSource? BitmapToBitmapSource(Bitmap bitmap)
    {
      if (bitmap == null) return null;

      try
      {
        using (var memoryStream = new MemoryStream())
        {
          bitmap.Save(memoryStream, ImageFormat.Png);
          memoryStream.Position = 0;

          BitmapImage bitmapImage = new BitmapImage();
          bitmapImage.BeginInit();
          bitmapImage.StreamSource = memoryStream;
          bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
          bitmapImage.EndInit();
          bitmapImage.Freeze(); // 스레드 간 공유를 위해 Freeze

          return bitmapImage;
        }
      }
      catch
      {
        return null;
      }
    }

    public List<WindowInfo> GetWindowsWithScreenshots()
    {
      var windows = GetAllWindows();

      foreach (var window in windows)
      {
        window.Screenshot = CaptureWindow(window.Handle);
      }

      return windows.Where(w => w.Screenshot != null).ToList();
    }

    public WindowInfo? GetDetailedWindowInfo(IntPtr hWnd)
    {
      if (!User32.IsWindow(hWnd))
        return null;

      int length = User32.GetWindowTextLength(hWnd);
      var title = new StringBuilder(length + 1);
      User32.GetWindowText(hWnd, title, title.Capacity);

      var className = new StringBuilder(256);
      User32.GetClassName(hWnd, className, className.Capacity);

      User32.GetWindowThreadProcessId(hWnd, out uint processId);
      string processName = string.Empty;

      try
      {
        var process = Process.GetProcessById((int)processId);
        processName = process.ProcessName;
      }
      catch { }

      User32.GetWindowRect(hWnd, out var rect);

      return new WindowInfo
      {
        Handle = hWnd,
        Title = title.ToString(),
        ClassName = className.ToString(),
        ProcessName = processName,
        ProcessId = (int)processId,
        IsVisible = User32.IsWindowVisible(hWnd),
        Width = rect.Width,
        Height = rect.Height,
        Screenshot = CaptureWindow(hWnd)
      };
    }
  }
}
