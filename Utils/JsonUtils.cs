using System.Text.Encodings.Web;
using System.Text.Json;

namespace SpeechAgent.Utils
{
  internal class JsonUtils
  {
    public static JsonSerializerOptions DefaultOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
  }
}
