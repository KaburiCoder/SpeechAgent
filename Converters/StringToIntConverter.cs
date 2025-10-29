using System.Globalization;
using System.Windows.Data;

namespace SpeechAgent.Converters
{
  public class StringToIntConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is int intValue)
      {
        return intValue.ToString();
      }
      return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string stringValue)
      {
        // 빈 문자열인 경우 0 반환
        if (string.IsNullOrWhiteSpace(stringValue))
        {
          return 0;
        }

        // Int32 범위 내에서 파싱 시도
        if (int.TryParse(stringValue, out int result))
        {
          return result;
        }

        // 파싱 실패 또는 범위 초과 시 0 반환
        return 0;
      }
      return 0;
    }
  }
}
