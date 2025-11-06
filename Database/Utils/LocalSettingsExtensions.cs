using SpeechAgent.Database.Schemas;

namespace SpeechAgent.Database.Utils
{
  public static class LocalSettingsExtensions
  {
    public static Rectangle ParseCustomImageRect(this LocalSettings settings)
    {
      if (string.IsNullOrWhiteSpace(settings.CustomImageRect))
      {
        throw new ArgumentException(
          "CustomImageRect is empty or null.",
          nameof(settings.CustomImageRect)
        );
      }

      var parts = settings.CustomImageRect.Split(',');
      if (parts.Length != 4)
      {
        throw new FormatException(
          "CustomImageRect must contain exactly 4 values (x,y,width,height)."
        );
      }

      try
      {
        return new Rectangle
        {
          X = int.Parse(parts[0].Trim()),
          Y = int.Parse(parts[1].Trim()),
          Width = int.Parse(parts[2].Trim()),
          Height = int.Parse(parts[3].Trim()),
        };
      }
      catch (FormatException ex)
      {
        throw new FormatException("Invalid number format in CustomImageRect.", ex);
      }
      catch (OverflowException ex)
      {
        throw new OverflowException("Number in CustomImageRect is too large or too small.", ex);
      }
    }
  }
}
