using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeechAgent.Messages
{
  public class SendToSettingsMessage(
    string exeTitle,
    string chartControlType,
    string chartIndex,
    string chartRegex,
    string chartRegexIndex,
    string nameControlType,
    string nameIndex,
    string nameRegex,
    string nameRegexIndex
  )
    : ValueChangedMessage<(
      string ExeTitle,
      string ChartControlType,
      string ChartIndex,
      string ChartRegex,
      string ChartRegexIndex,
      string NameControlType,
      string NameIndex,
      string NameRegex,
      string NameRegexIndex
    )>(
      (
        exeTitle,
        chartControlType,
        chartIndex,
        chartRegex,
        chartRegexIndex,
        nameControlType,
        nameIndex,
        nameRegex,
        nameRegexIndex
      )
    ) { }
}
