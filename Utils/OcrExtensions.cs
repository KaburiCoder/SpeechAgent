using System.Windows.Media.Imaging;
using Tesseract;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// BitmapSource OCR ���� Ȯ�� �޼���
  /// </summary>
  public static class OcrExtensions
  {
    private const string TessdataPath = @"./tessdata";
    private const string Languages = "eng+kor";

    /// <summary>
    /// BitmapSource���� �ؽ�Ʈ�� �����մϴ� (Tesseract OCR ���).
    /// �޸� ���� ����: �ӽ� ������ �ڵ����� �����˴ϴ�.
    /// </summary>
    public static string Ocr(this BitmapSource bitmapSource)
    {
      try
      {
        string? tempFilePath = bitmapSource.ToTempFile();

        if (tempFilePath == null)
          return string.Empty;

        try
        {
          return ExtractTextFromFile(tempFilePath);
        }
        finally
        {
          // �ӽ� ���� ����
          BitmapSourceExtensions.DeleteTempFile(tempFilePath);
        }
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// ���� ��ο��� �ؽ�Ʈ�� �����մϴ�.
    /// </summary>
    private static string ExtractTextFromFile(string filePath)
    {
      try
      {
        using (var engine = new TesseractEngine(TessdataPath, Languages, EngineMode.Default))
        {
          using (var img = Pix.LoadFromFile(filePath))
          {
            using (var page = engine.Process(img))
            {
              return page.GetText();
            }
          }
        }
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
    }
  }
}
