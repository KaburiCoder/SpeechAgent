using System.Windows.Media.Imaging;

namespace SpeechAgent.Models
{
  public class PatientImageResult
  {
    public PatientInfo? PatientInfo { get; set; }
    public BitmapSource? BitmapSource { get; set; }

    public void SetResult(PatientInfo? patientInfo, BitmapSource? bitmapSource)
    {
      PatientInfo = patientInfo;
      BitmapSource = bitmapSource;
    }

    public void Clear()
    {
      PatientInfo = null;
      BitmapSource = null;
    }
  }
}
