using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace SpeechAgent.Utils
{
  public static class BitmapSourceExtensions
  {
    /// <summary>
    /// BitmapSource�� Bitmap���� ��ȯ�մϴ�.
    /// �޸� ���� ����: ��ȯ�� Bitmap�� using ������ �����Ǿ�� �մϴ�.
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
    /// BitmapSource�� �ӽ� PNG ���Ϸ� �����ϰ� ��θ� ��ȯ�մϴ�.
    /// �޸� ���� ����: �ӽ� ������ ��� �� �������� �����ؾ� �մϴ�.
    /// </summary>
    public static string? ToTempFile(this BitmapSource bitmapSource)
    {
      try
      {
        if (bitmapSource == null)
          return null;

        using (var bitmap = bitmapSource.ToBitmap())
        {
          if (bitmap == null)
            return null;

          string tempFilePath = System.IO.Path.Combine(
                  System.IO.Path.GetTempPath(),
                $"ocr_{Guid.NewGuid()}.png");

          bitmap.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);
          return tempFilePath;
        }
      }
      catch(Exception ex)
      {
        LogUtils.WriteTextLog("Error.log", ex.ToString(), append: true);
        return null;
      }
    }

    /// <summary>
    /// �����ϰ� �ӽ� ������ �����մϴ�.
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
        // ���� - �ӽ� ���� ���� ���д� �ɰ����� ����
      }
    }
  }
}
