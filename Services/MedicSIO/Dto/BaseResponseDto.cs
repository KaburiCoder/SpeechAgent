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

  public class BaseResponseWithDataDto<T> : BaseResponseDto
  {
    [JsonPropertyName("data")]
    public T? Data { get; set; }
  }
}
