using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeechAgent.Messages
{
  public class SendToSettingsMessage(string exeTitle, string chartClass, string chartIndex, string nameClass, string nameIndex) : ValueChangedMessage<(string ExeTitle, string ChartClass, string ChartIndex, string NameClass, string NameIndex)>((exeTitle, chartClass, chartIndex, nameClass, nameIndex))
  {
  }
}