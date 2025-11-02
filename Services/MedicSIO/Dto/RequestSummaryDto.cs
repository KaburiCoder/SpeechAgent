using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpeechAgent.Services.MedicSIO.Dto
{
  public class RequestSummaryDto
  {
    [JsonPropertyName("key")]
    public string Key { get; set; } = ""; // 'all', 'cc', 's', 'o', 'a', 'p'
  }

  public class RequestSummaryResponseDto
  {
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
  }
}
