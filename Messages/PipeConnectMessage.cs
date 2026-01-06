using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeechAgent.Messages
{
  public class PipeConnectData
  {
    public bool IsConnected { get; }
    public PipeConnectData(bool isConnected)
    {
      IsConnected = isConnected;
    }
  }

  public class PipeConnectMessage(PipeConnectData data) : ValueChangedMessage<PipeConnectData>(data)
  {
  }
}
