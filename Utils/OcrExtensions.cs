using System.Windows.Media.Imaging;
using Tesseract;
using OpenCvSharp;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// BitmapSource OCR ���� Ȯ�� �޼���
  /// </summary>
  public static class OcrExtensions
  {
    private const string TessdataPath = @"./tessdata";
    private const string Languages = "eng";

    /// <summary>
    /// BitmapSource���� �ؽ�Ʈ�� �����մϴ� (Tesseract OCR ���).
    /// �޸� ���� ����: �ӽ� ������ �ڵ����� �����˴ϴ�.
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

            // ��ó�� �̹����� �ӽ� ���Ϸ� ����
            string preprocessedPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"ocr_pre_{Guid.NewGuid()}.png");
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
          // �ӽ� ���� ����
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
    /// ���� ��ο��� �ؽ�Ʈ�� �����մϴ�.
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
    /// �̹��� ������ ��ó���մϴ� (OCR ��Ȯ�� ���).
    /// 1. ũ�� ���� Ȯ�� (���� �ս� �ּ�ȭ)
    /// 2. ������׷� ��Ȱȭ�� ��� ���
    /// 3. Adaptive Threshold (���� ���ڿ� ����)
    /// 4. MedianBlur�� ���� �Ǵ� Ŀ�� 1�� ���ϰ�
    /// 5. Morphology�� ���� �Ǵ� Ŀ�� 1x1
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
        //1. �÷��� �б�
        src = Cv2.ImRead(imagePath, ImreadModes.Color);
        if (src.Empty())
          return new Mat();

        //2. BGR �� HSV ��ȯ
        hsv = new Mat();
        Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);

        //3. �Ķ��� �迭 ���� ���� (H:100~140, S/V:50~255)
        Scalar lowerBlue = new Scalar(100, 50, 50);
        Scalar upperBlue = new Scalar(140, 255, 255);

        //4. �Ķ��� �迭�� ����ŷ
        mask = new Mat();
        Cv2.InRange(hsv, lowerBlue, upperBlue, mask);

        //5. ����ũ ����: �Ķ���(����)�� ����, �������� ���
        result = new Mat();
        Cv2.BitwiseNot(mask, result);

        //6. ����40% crop
        int cropX = (int)(result.Cols * 0.4);
        int cropWidth = result.Cols - cropX;
        if (cropWidth <= 0) cropWidth = result.Cols; // ������ġ
        cropped = new Mat(result, new OpenCvSharp.Rect(cropX, 0, cropWidth, result.Rows));

        //7. ũ�� Ȯ�� (5��)
        double scale = 5.0;
        resized = new Mat();
        Cv2.Resize(cropped, resized, new OpenCvSharp.Size(0, 0), scale, scale, InterpolationFlags.Cubic);

        return resized;
      }
      finally
      {
        src?.Dispose();
        hsv?.Dispose();
        mask?.Dispose();
        result?.Dispose();
        cropped?.Dispose();
        // resized�� ��ȯ�ϹǷ� Dispose���� ����
      }
    }

    public static string? Test()
    {
      string imagePath = "D:\\Apps\\SpeechAgent\\Assets\\Usarang.png";
      string savePath = "D:\\Apps\\SpeechAgent\\Assets\\Usarang_preprocessed.png";

      try
      {
        // �̹��� ��ó��
        using (var preprocessedMat = PreprocessImage(imagePath))
        {
          if (preprocessedMat.Empty())
            return null;

          // ��ó�� �̹��� ����
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
