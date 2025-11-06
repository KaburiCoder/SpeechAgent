using System.Windows.Media.Imaging;
using OpenCvSharp;
using Tesseract;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// BitmapSource OCR 관련 확장 메서드
  /// </summary>
  public static class OcrExtensions
  {
    private const string TessdataPath = @"./tessdata";
    private const string Languages = "eng";

    /// <summary>
    /// BitmapSource에서 텍스트를 추출합니다 (Tesseract OCR 사용).
    /// 메모리 누수 방지: 임시 파일은 자동으로 정리됩니다.
    /// </summary>
    public static string OcrUSarangChart(this BitmapSource bitmapSource)
    {
      try
      {
        string? tempFilePath = bitmapSource.ToTempFile();

        if (tempFilePath == null)
          return string.Empty;

        try
        {
          using (var preprocessedMat = PreprocessImage(tempFilePath))
          {
            if (preprocessedMat.Empty())
              return string.Empty;

            // 전처리 이미지를 임시 파일로 저장
            string preprocessedPath = System.IO.Path.Combine(
              System.IO.Path.GetTempPath(),
              $"ocr_pre_{Guid.NewGuid()}.png"
            );
            Cv2.ImWrite(preprocessedPath, preprocessedMat);
            try
            {
              return ExtractTextFromFile(preprocessedPath, onlyNumber: true);
            }
            finally
            {
              BitmapSourceExtensions.DeleteTempFile(preprocessedPath);
            }
          }
        }
        finally
        {
          // 임시 파일 삭제
          BitmapSourceExtensions.DeleteTempFile(tempFilePath);
        }
      }
      catch (Exception ex)
      {
        LogUtils.WriteTextLog("Error.log", ex.ToString(), append: true);
        return string.Empty;
      }
    }

    /// <summary>
    /// 파일 경로에서 텍스트를 추출합니다.
    /// </summary>
    private static string ExtractTextFromFile(string filePath, bool onlyNumber = false)
    {
      try
      {
        using (var engine = new TesseractEngine(TessdataPath, Languages, EngineMode.Default))
        {
          if (onlyNumber)
          {
            engine.SetVariable("tessedit_char_whitelist", "0123456789");
            engine.DefaultPageSegMode = PageSegMode.SingleLine;
          }
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

    /// <summary>
    /// 이미지 파일을 전처리합니다 (OCR 정확도 향상).
    /// 1. 크기 먼저 확대 (정보 손실 최소화)
    /// 2. 히스토그램 평활화로 대비 향상
    /// 3. Adaptive Threshold (얇은 숫자에 유리)
    /// 4. MedianBlur는 생략 또는 커널 1로 약하게
    /// 5. Morphology는 생략 또는 커널 1x1
    /// </summary>
    public static Mat PreprocessImage(string imagePath)
    {
      Mat? src = null;
      Mat? hsv = null;
      Mat? mask = null;
      Mat? result = null;
      Mat? cropped = null;
      Mat? resized = null;
      try
      {
        //1. 컬러로 읽기
        src = Cv2.ImRead(imagePath, ImreadModes.Color);
        if (src.Empty())
          return new Mat();

        //2. BGR → HSV 변환
        hsv = new Mat();
        Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);

        //3. 파란색 계열 범위 지정 (H:100~140, S/V:50~255)
        Scalar lowerBlue = new Scalar(100, 50, 50);
        Scalar upperBlue = new Scalar(140, 255, 255);

        //4. 파란색 계열만 마스킹
        mask = new Mat();
        Cv2.InRange(hsv, lowerBlue, upperBlue, mask);

        //5. 마스크 반전: 파란색(글자)은 검정, 나머지는 흰색
        result = new Mat();
        Cv2.BitwiseNot(mask, result);

        //6. 좌측40% crop
        int cropX = (int)(result.Cols * 0.4);
        int cropWidth = result.Cols - cropX;
        if (cropWidth <= 0)
          cropWidth = result.Cols; // 안전장치
        cropped = new Mat(result, new OpenCvSharp.Rect(cropX, 0, cropWidth, result.Rows));

        //7. 크기 확대 (5배)
        double scale = 5.0;
        resized = new Mat();
        Cv2.Resize(
          cropped,
          resized,
          new OpenCvSharp.Size(0, 0),
          scale,
          scale,
          InterpolationFlags.Cubic
        );

        return resized;
      }
      finally
      {
        src?.Dispose();
        hsv?.Dispose();
        mask?.Dispose();
        result?.Dispose();
        cropped?.Dispose();
        // resized는 반환하므로 Dispose하지 않음
      }
    }

    public static string? Test()
    {
      string imagePath = "D:\\Apps\\SpeechAgent\\Assets\\Usarang.png";
      string savePath = "D:\\Apps\\SpeechAgent\\Assets\\Usarang_preprocessed.png";

      try
      {
        // 이미지 전처리
        using (var preprocessedMat = PreprocessImage(imagePath))
        {
          if (preprocessedMat.Empty())
            return null;

          // 전처리 이미지 저장
          Cv2.ImWrite(savePath, preprocessedMat);

          using (var engine = new TesseractEngine(TessdataPath, Languages, EngineMode.Default))
          {
            engine.SetVariable("tessedit_char_whitelist", "0123456789");
            engine.DefaultPageSegMode = PageSegMode.SingleLine;

            using (var pix = Pix.LoadFromMemory(preprocessedMat.ImEncode(".png")))
            {
              using (var page = engine.Process(pix))
              {
                return page.GetText();
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        return null;
      }
    }
  }
}
