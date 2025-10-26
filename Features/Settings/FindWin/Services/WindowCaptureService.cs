using SpeechAgent.Features.Settings.FindWin.Models;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using Vanara.PInvoke;

namespace SpeechAgent.Features.Settings.FindWin.Services
{
  public interface IWindowCaptureService
  {
    List<WindowInfo> GetAllWindows();
    BitmapSource? CaptureWindow(IntPtr hWnd);
    BitmapSource? CaptureWindow(IntPtr hWnd, Rectangle? captureRect);
    List<WindowInfo> GetWindowsWithScreenshots();
    WindowInfo? GetDetailedWindowInfo(IntPtr hWnd);
  }

  internal class WindowCaptureService : IWindowCaptureService
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
      return CaptureWindow(hWnd, null);
    }

    // 특정 영역을 캡처하는 오버로드
    public BitmapSource? CaptureWindow(IntPtr hWnd, Rectangle? captureRect)
    {
      try
      {
        if (!User32.IsWindow(hWnd))
          return null;

        RECT rect;
        var result = DwmApi.DwmGetWindowAttribute(new HWND(hWnd),
            DwmApi.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out rect);

        if (result.Failed)
        {
          User32.GetWindowRect(hWnd, out rect);
        }

        int windowWidth = rect.Width;
        int windowHeight = rect.Height;

        if (windowWidth <= 0 || windowHeight <= 0)
          return null;

        // 전체 윈도우 캡처
        using (Bitmap fullBitmap = new Bitmap(windowWidth, windowHeight))
        {
          using (Graphics graphics = Graphics.FromImage(fullBitmap))
          {
            var hdc = graphics.GetHdc();
            Gdi32.SafeHDC safeHdc = new Gdi32.SafeHDC(hdc, false);
            User32.PrintWindow(new HWND(hWnd), safeHdc, User32.PW.PW_RENDERFULLCONTENT);
            graphics.ReleaseHdc(hdc);
          }

          // 특정 영역이 지정된 경우 해당 영역만 추출
          if (captureRect.HasValue)
          {
            var crop = captureRect.Value;

            // 영역이 윈도우 범위를 벗어나지 않도록 조정
            crop.X = Math.Max(0, Math.Min(crop.X, windowWidth));
            crop.Y = Math.Max(0, Math.Min(crop.Y, windowHeight));
            crop.Width = Math.Min(crop.Width, windowWidth - crop.X);
            crop.Height = Math.Min(crop.Height, windowHeight - crop.Y);

            if (crop.Width <= 0 || crop.Height <= 0)
              return null;

            using (Bitmap croppedBitmap = fullBitmap.Clone(crop, fullBitmap.PixelFormat))
            {
              return BitmapToBitmapSource(croppedBitmap);
            }
          }

          return BitmapToBitmapSource(fullBitmap);
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
