using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpeechAgent.Services.MedicSIO.Dto
{
  public class PingFromWebDto
  {
    [JsonPropertyName("targetAppName")]
    public string TargetAppName { get; set; } = string.Empty;
  }
}
