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
    // 단일파일(self-extract) 게시 시 tessdata는 exe 옆이 아니라 임시 추출폴더(AppContext.BaseDirectory)에 풀린다.
    // 일반 빌드에서도 BaseDirectory가 exe 폴더이므로 양쪽 모두 정확히 맞는다. ("./tessdata" 상대경로는 단일파일에서 실패)
    private static readonly string TessdataPath =
      System.IO.Path.Combine(AppContext.BaseDirectory, "tessdata");
    private const string Languages = "eng";

    // tessdata 경로/존재여부를 세션당 1회만 로그로 남기기 위한 가드 (단일파일 게시 검증용)
    private static int _tessdataStatusLogged;

    /// <summary>
    /// BitmapSource에서 텍스트를 추출합니다 (Tesseract OCR 사용).
    /// 메모리 누수 방지: 임시 파일은 자동으로 정리됩니다.
    /// </summary>
    public static string OcrUSarangChart(this BitmapSource bitmapSource)
    {
      try
      {
        LogTessdataStatusOnce();

        string? tempFilePath = bitmapSource.ToTempFile();

        if (tempFilePath == null)
          return string.Empty;

        try
        {
          using var preprocessedMat = PreprocessImage(tempFilePath);

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
            string result = ExtractTextFromFile(preprocessedPath, onlyNumber: true);
            LogUtils.WriteLog(
              LogLevel.Debug,
              $"[OCR] 의사랑 차트 인식 결과='{result.Trim()}' (len={result.Length})");
            return result;
          }
          finally
          {
            BitmapSourceExtensions.DeleteTempFile(preprocessedPath);
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
        LogUtils.WriteLog(LogLevel.Error, ex.ToString());
        return string.Empty;
      }
    }

    /// <summary>
    /// tessdata 폴더 경로와 존재 여부를 세션당 1회 로그로 남깁니다.
    /// 단일파일 게시 시 OCR이 정상 위치(AppContext.BaseDirectory\tessdata)를 찾는지 확인하기 위함입니다.
    /// </summary>
    private static void LogTessdataStatusOnce()
    {
      if (Interlocked.Exchange(ref _tessdataStatusLogged, 1) != 0)
        return;

      bool dirExists = System.IO.Directory.Exists(TessdataPath);
      bool engExists = System.IO.File.Exists(
        System.IO.Path.Combine(TessdataPath, "eng.traineddata"));
      LogUtils.WriteLog(
        dirExists && engExists ? LogLevel.Info : LogLevel.Error,
        $"[OCR] tessdata 경로='{TessdataPath}' (폴더존재={dirExists}, eng.traineddata={engExists})");
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
        LogUtils.WriteLog(LogLevel.Error, $"[OCR] ExtractTextFromFile 실패: {ex}");
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
