using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SpeechAgent.Models;

namespace SpeechAgent.Messages
{
  public class MedicSIOConnectionChangedMessage(bool value) : ValueChangedMessage<bool>(value) { }

  public class MedicSIOJoinRoomChangedMessage(bool value) : ValueChangedMessage<bool>(value) { }

  public class WebPingReceivedMessage(DateTime value) : ValueChangedMessage<DateTime>(value) { }
}
