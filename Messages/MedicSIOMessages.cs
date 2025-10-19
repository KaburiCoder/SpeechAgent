using CommunityToolkit.Mvvm.Messaging.Messages;
using SpeechAgent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAgent.Messages
{
  public class MedicSIOConnectionChangedMessage : ValueChangedMessage<bool>
  {
    public MedicSIOConnectionChangedMessage(bool value) : base(value) { }
  }

  public class MedicSIOJoinRoomChangedMessage : ValueChangedMessage<bool>
  {
    public MedicSIOJoinRoomChangedMessage(bool value) : base(value) { }
  }
}
