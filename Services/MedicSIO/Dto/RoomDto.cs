using System.Text.Json.Serialization;

namespace SpeechAgent.Services.MedicSIO.Dto
{
  public class RoomDto
  {
    [JsonPropertyName("roomId")]
    public string RoomId { get; set; } = ""; 
    [JsonPropertyName("to")]
    public string To { get; set; } = "";
  }
}
