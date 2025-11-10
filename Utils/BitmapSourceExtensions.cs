using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace SpeechAgent.Utils
{
  public static class BitmapSourceExtensions
  {
    public enum ImageFormat
    {
      Png,
      Jpeg,
      Bmp,
      Tiff,
      Gif,
    }

    public static void SaveBitmapSourceToFile(
      this BitmapSource bitmapSource,
      string filePath,
      ImageFormat imageFormat = ImageFormat.Png
    )
    {
      BitmapEncoder encoder = imageFormat switch
      {
        ImageFormat.Png => new PngBitmapEncoder(),
        ImageFormat.Jpeg => new JpegBitmapEncoder(),
        ImageFormat.Bmp => new BmpBitmapEncoder(),
        ImageFormat.Tiff => new TiffBitmapEncoder(),
        ImageFormat.Gif => new GifBitmapEncoder(),
        _ => new PngBitmapEncoder(),
      };

      encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

      using (FileStream fs = new FileStream(filePath, FileMode.Create))
      {
        encoder.Save(fs);
      }
    }

    /// <summary>
    /// BitmapSource를 Bitmap으로 변환합니다.
    /// 메모리 누수 방지: 반환된 Bitmap은 using 문에서 관리되어야 합니다.
    /// </summary>
    public static Bitmap? ToBitmap(this BitmapSource bitmapSource)
    {
      try
      {
        if (bitmapSource == null)
          return null;

        using (var memoryStream = new System.IO.MemoryStream())
        {
          var encoder = new PngBitmapEncoder();
          encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
          encoder.Save(memoryStream);
          memoryStream.Position = 0;
          return new Bitmap(memoryStream);
        }
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// BitmapSource를 임시 PNG 파일로 저장하고 경로를 반환합니다.
    /// 메모리 누수 방지: 임시 파일은 사용 후 수동으로 삭제해야 합니다.
    /// </summary>
    public static string? ToTempFile(this BitmapSource bitmapSource)
    {
      try
      {
        if (bitmapSource == null)
          return null;
        string tempFilePath = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}.png");
        bitmapSource.SaveBitmapSourceToFile(tempFilePath);
        return tempFilePath;
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Debug, $"[ToTempFile] {ex}");
        return null;
      }
    }

    /// <summary>
    /// 안전하게 임시 파일을 삭제합니다.
    /// </summary>
    public static void DeleteTempFile(string filePath)
    {
      if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
        return;

      try
      {
        System.IO.File.Delete(filePath);
      }
      catch
      {
        // 무시 - 임시 파일 삭제 실패는 심각하지 않음
      }
    }
  }
}
