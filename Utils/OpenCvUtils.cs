using System.Diagnostics;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace SpeechAgent.Utils
{
  public class OpenCvUtils
  {
    /// <summary>
    /// 두 이미지를 비교하여 유사도를 판단합니다. MSE(Mean Squared Error) 방식을 사용합니다. MSE 값이 낮을수록 이미지가 유사합니다.
    /// </summary>
    /// <param name="source1"></param>
    /// <param name="source2"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public static bool AreImagesSimilar(
      BitmapSource? source1,
      BitmapSource? source2,
      double threshold
    )
    {
      if (source1 == null || source2 == null)
        return false;

      using (Mat mat1 = BitmapSourceConverter.ToMat(source1))
      using (Mat mat2 = BitmapSourceConverter.ToMat(source2))
      {
        // 이미지 크기 확인
        if (mat1.Size() != mat2.Size() || mat1.Type() != mat2.Type())
        {
          Debug.Print("Images have different sizes or types.");
          return false;
        }

        // 그레이스케일 변환 (픽셀 비교는 보통 단일 채널로)
        using (Mat gray1 = new Mat())
        using (Mat gray2 = new Mat())
        {
          Cv2.CvtColor(mat1, gray1, ColorConversionCodes.BGR2GRAY);
          Cv2.CvtColor(mat2, gray2, ColorConversionCodes.BGR2GRAY);

          // 픽셀 단위 차이 계산 (절대 차이)
          using (Mat diff = new Mat())
          {
            Cv2.Absdiff(gray1, gray2, diff);

            // MSE 계산
            double mse = Cv2.Mean(diff.Mul(diff))[0]; // 제곱 후 평균 계산
            Debug.Print($"Image MSE: {mse}");

            // MSE가 작을수록 유사하므로, threshold 이하인지 확인
            return mse <= threshold;
          }
        }
      }
    }
  }
}
