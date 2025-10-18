using System.Text.Json.Serialization;

namespace SpeechAgent.Services.MedicSIO.Dto
{
  public class JoinRoomDto
  {
    [JsonPropertyName("roomId")]
    public string RoomId { get; set; } = "";
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = "agent";
  }
}
