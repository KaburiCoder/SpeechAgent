using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeechAgent.Messages
{
  public class SendToSettingsImageMessage(string customExeTitle, string customImageRect)
    : ValueChangedMessage<(string CustomExeTitle, string CustomImageRect)>(
      (customExeTitle, customImageRect)
    ) { }
}
