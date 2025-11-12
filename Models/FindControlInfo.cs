using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeechAgent.Services;

namespace SpeechAgent.Models
{
  public class FindControlInfo
  {
    public string ControlType { get; set; } = "ControlType.Edit";
    public int Index { get; set; } = 0;
    public RegexInfo? Regex { get; set; } = null;
  }
}
