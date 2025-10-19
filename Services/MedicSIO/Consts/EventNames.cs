using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAgent.Services.MedicSIO.Consts
{
  public class EventNames
  {
    #region Send Events
    public const string JoinRoom = "joinRoom";
    public const string LeaveRoom = "leaveRoom";
    public const string SendPatientInfo = "sendPatientInfo";
    #endregion

    #region Receive Events
    public const string PingFromWeb = "pingFromWeb";
    #endregion
  }
}
