using System.Globalization;
using System.Windows.Data;

namespace SpeechAgent.Converters
{
  public class InverseBooleanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return !boolValue; // bool 반전
      }
      return value; // 기본값 반환
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return !boolValue; // 양방향 바인딩 지원 (필요 시)
      }
      return value;
    }
  }
}
