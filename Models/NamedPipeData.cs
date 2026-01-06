using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAgent.Models
{
  public class NamedPipeAction
  {
    public static readonly string LOAD_PATIENT = "LOAD_PATIENT";
  }

  public record NamedPipeData(string Action, object Payload) { }
}
