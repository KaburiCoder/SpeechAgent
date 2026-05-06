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
    public const string CURRENT_CHART = "CURRENT_CHART";
  }

  public class NamePipeReceiveAction
  {
    public const string PING = "PING";
    public const string OPEN_SETTINGS = "OPEN_SETTINGS";
    public const string GET_CURRENT_CHART = "GET_CURRENT_CHART";
  }

  public record NamedPipeData(string Action, object Payload) { }
}
