using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAgent.Models
{
  public class NamedPipeAction
  {
    public const string PONG = "PONG";
    public const string LOAD_PATIENT = "LOAD_PATIENT";
  }

  public class NamePipeReceiveAction
  {
    public const string PING = "PING";
    public const string OPEN_SETTINGS = "OPEN_SETTINGS";
  }

  public record NamedPipeData(string Action, object Payload) { }
}
