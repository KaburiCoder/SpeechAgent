using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SpeechAgent.Utils.Converters
{
  public static class ImageConverter
  {
    public static string ToDataUrl(this BitmapSource bitmapSource, string format = "png")
    {
      // BitmapSource를 PNG/JPEG로 인코딩
      BitmapEncoder encoder = format.ToLower() switch
      {
        "jpeg" => new JpegBitmapEncoder(),
        _ => new PngBitmapEncoder() // 기본값은 PNG
      };
      encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

      using var memoryStream = new MemoryStream();
      encoder.Save(memoryStream);
      byte[] imageBytes = memoryStream.ToArray();

      // Base64로 변환하고 데이터 URL 생성
      string base64String = Convert.ToBase64String(imageBytes);
      return $"data:image/{format};base64,{base64String}";
    }
  }
}
