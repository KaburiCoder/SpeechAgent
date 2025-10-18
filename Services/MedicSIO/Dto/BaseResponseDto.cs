using System.Text.Json.Serialization;

namespace SpeechAgent.Services.MedicSIO.Dto
{
  public class BaseResponseDto
  {
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("message")]
    public string? Message { get; set; }
  }
}
