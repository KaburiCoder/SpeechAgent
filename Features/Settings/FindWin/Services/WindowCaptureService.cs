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

    // Ư�� ������ ĸó�ϴ� �����ε�
    public BitmapSource? CaptureWindow(IntPtr hWnd, Rectangle? captureRect)
    {
      try
      {
        if (!User32.IsWindow(hWnd))
          return null;

        // ���� DC ��� ĸó �õ�
        var dcBitmap = TryCaptureWindowDC(hWnd, captureRect);
        if (dcBitmap != null)
          return dcBitmap;

        // DC ĸó ���� �� DWM ��� ĸó �õ�
        return TryCaptureWindowDWM(hWnd, captureRect);
      }
      catch
      {
        return null;
      }
    }

    private BitmapSource? TryCaptureWindowDC(IntPtr hWnd, Rectangle? captureRect)
    {
      try
      {
        // ������ ȭ�� ��ǥ
        User32.GetWindowRect(hWnd, out var rect);
        int windowWidth = rect.Width;
        int windowHeight = rect.Height;

        if (windowWidth <= 0 || windowHeight <= 0)
          return null;

        // ȭ�� DC�� ���� ���� ĸó
        var hdc = User32.GetWindowDC(new HWND(hWnd));
        if (hdc == IntPtr.Zero)
          return null;

        try
        {
          using (Bitmap fullBitmap = new Bitmap(windowWidth, windowHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
          {
            using (Graphics g = Graphics.FromImage(fullBitmap))
            {
              // ������ DC���� ��Ʈ������ ���� ����
              var hdcDest = g.GetHdc();
              Gdi32.BitBlt(new Gdi32.SafeHDC(hdcDest, false), 0, 0, windowWidth, windowHeight,
                            new Gdi32.SafeHDC(hdc.DangerousGetHandle(), false), 0, 0, Gdi32.RasterOperationMode.SRCCOPY);
              g.ReleaseHdc(hdcDest);
            }

            // ��Ʈ���� ��ȿ���� ���� (������ ���������� Ȯ��)
            if (!IsValidBitmap(fullBitmap))
              return null;

            return ProcessBitmap(fullBitmap, windowWidth, windowHeight, captureRect);
          }
        }
        finally
        {
          User32.ReleaseDC(new HWND(hWnd), hdc);
        }
      }
      catch
      {
        return null;
      }
    }

    private BitmapSource? TryCaptureWindowDWM(IntPtr hWnd, Rectangle? captureRect)
    {
      try
      {
        // DWM ������ ���� ȹ��
        var dwmResult = DwmApi.DwmGetWindowAttribute(new HWND(hWnd),
     DwmApi.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRect);

        if (dwmResult.Failed)
        {
          User32.GetWindowRect(hWnd, out dwmRect);
        }

        int windowWidth = dwmRect.Width;
        int windowHeight = dwmRect.Height;

        if (windowWidth <= 0 || windowHeight <= 0)
          return null;

        // ��ü ������ ĸó
        using (Bitmap fullBitmap = new Bitmap(windowWidth, windowHeight))
        {
          using (Graphics graphics = Graphics.FromImage(fullBitmap))
          {
            var hdc = graphics.GetHdc();
            Gdi32.SafeHDC safeHdc = new Gdi32.SafeHDC(hdc, false);
            User32.PrintWindow(new HWND(hWnd), safeHdc, User32.PW.PW_RENDERFULLCONTENT);
            graphics.ReleaseHdc(hdc);
          }

          return ProcessBitmap(fullBitmap, windowWidth, windowHeight, captureRect);
        }
      }
      catch
      {
        return null;
      }
    }

    private BitmapSource? ProcessBitmap(Bitmap fullBitmap, int windowWidth, int windowHeight, Rectangle? captureRect)
    {
      // Ư�� ������ ������ ��� �ش� ������ ����
      if (captureRect.HasValue)
      {
        var crop = captureRect.Value;

        // ������ ������ ������ ����� �ʵ��� ����
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

    private bool IsValidBitmap(Bitmap bitmap)
    {
      try
      {
        // ���ø����� ��Ʈ���� ������ ���������� Ȯ��
        // ������ �������̸� DC ĸó ���з� ����
        int sampleCount = 0;
        int blackCount = 0;

        for (int x = 0; x < bitmap.Width; x += Math.Max(1, bitmap.Width / 10))
        {
          for (int y = 0; y < bitmap.Height; y += Math.Max(1, bitmap.Height / 10))
          {
            var pixel = bitmap.GetPixel(x, y);
            if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
              blackCount++;
            sampleCount++;
          }
        }

        // ������ 80% �̻��� �������̸� ��ȿ���� ���� ��Ʈ��
        return blackCount < (sampleCount * 0.8);
      }
      catch
      {
        return true; // ���� ���� �� ��ȿ�� ������ ����
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
          bitmapImage.Freeze(); // ������ �� ������ ���� Freeze

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
