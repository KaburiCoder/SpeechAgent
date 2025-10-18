using System;
using System.Text.Json.Serialization;

namespace SpeechAgent.Services.MedicSIO.Dto
{
  public class PatientInfoDto
  {
    [JsonPropertyName("chart")]
    public string Chart { get; set; } = "";
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
  }
}
