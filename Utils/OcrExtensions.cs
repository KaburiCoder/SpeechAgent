using System.Windows.Media.Imaging;
using Tesseract;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// BitmapSource OCR 관련 확장 메서드
  /// </summary>
  public static class OcrExtensions
  {
    private const string TessdataPath = @"./tessdata";
    private const string Languages = "eng+kor";

    /// <summary>
    /// BitmapSource에서 텍스트를 추출합니다 (Tesseract OCR 사용).
    /// 메모리 누수 방지: 임시 파일은 자동으로 정리됩니다.
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
          // 임시 파일 삭제
          BitmapSourceExtensions.DeleteTempFile(tempFilePath);
        }
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// 파일 경로에서 텍스트를 추출합니다.
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
