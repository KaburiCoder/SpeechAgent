using CommunityToolkit.Mvvm.Messaging.Messages;
using SpeechAgent.Database.Schemas;

namespace SpeechAgent.Messages
{
  public class LocalSettingsChangeData
  {
    public LocalSettings Settings { get; }
    public LocalSettings? PreviousSettings { get; }
    public LocalSettingsChangeData(LocalSettings settings, LocalSettings? previousSettings)
    {
      Settings = settings;
      PreviousSettings = previousSettings;
    }
  }

  public class LocalSettingsChangedMessage(LocalSettingsChangeData data) : ValueChangedMessage<LocalSettingsChangeData>(data) { }
}
