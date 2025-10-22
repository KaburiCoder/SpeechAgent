using System.Globalization;
using System.Windows.Data;

namespace SpeechAgent.Converters
{
  public class BooleanToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue && parameter is string param)
      {
        var parts = param.Split(',');
        if (parts.Length >= 2)
        {
          return boolValue ? parts[0] : parts[1];
        }
        
        // 기존 콜론 구분자도 지원
        var colonParts = param.Split(':');
        if (colonParts.Length >= 2)
        {
          return boolValue ? colonParts[1] : colonParts[2];
        }
      }
      return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}