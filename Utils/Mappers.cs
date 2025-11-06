using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpeechAgent.Utils
{
  public static class Mappers
  {
    public static T DeepCopy<T>(this T source)
    {
      var json = JsonSerializer.Serialize(source);
      return JsonSerializer.Deserialize<T>(json)!;
    }
  }
}
