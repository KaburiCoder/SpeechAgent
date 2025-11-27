using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAgent.Constants
{
  public class ApiConfig
  {
    public const string SpeechUserKey = "x-user-key";
#if DEBUG
    public const string SpeechBaseUrl = "http://localhost:3100/api/";
    public const string SocketBaseUrl = "http://localhost:3100/agent";

    //public const string SocketBaseUrl = "https://medic.clickcns.com/agent";
    //public const string SpeechBaseUrl = "https://medic.clickcns.com/api/";
#else
    public const string SocketBaseUrl = "https://medic.clickcns.com/agent";
    public const string SpeechBaseUrl = "https://medic.clickcns.com/api/";
#endif
  }
}
