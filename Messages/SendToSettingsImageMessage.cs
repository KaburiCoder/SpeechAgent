using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeechAgent.Messages
{
  public class SendToSettingsImageMessage(string customImageName, string customImageRect) 
    : ValueChangedMessage<(string CustomImageName, string CustomImageRect)>((customImageName, customImageRect))
{
  }
}
