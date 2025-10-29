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
        // �� ���ڿ��� ��� 0 ��ȯ
        if (string.IsNullOrWhiteSpace(stringValue))
        {
          return 0;
        }

        // Int32 ���� ������ �Ľ� �õ�
        if (int.TryParse(stringValue, out int result))
        {
          return result;
        }

        // �Ľ� ���� �Ǵ� ���� �ʰ� �� 0 ��ȯ
        return 0;
      }
      return 0;
    }
  }
}
