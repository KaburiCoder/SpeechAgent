using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeechAgent.Messages
{
  public class SendToSettingsImageMessage(string customImageClass, string customImageRect) 
    : ValueChangedMessage<(string CustomImageClass, string CustomImageRect)>((customImageClass, customImageRect))
{
  }
}
