using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeechAgent.Messages
{
  public class SendToSettingsMessage(string exeTitle, string chartControlType, string chartIndex, string nameControlType, string nameIndex) : ValueChangedMessage<(string ExeTitle, string ChartControlType, string ChartIndex, string NameControlType, string NameIndex)>((exeTitle, chartControlType, chartIndex, nameControlType, nameIndex))
  {
  }
}