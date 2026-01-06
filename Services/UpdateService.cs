using SpeechAgent.Constants;
using System.Windows.Threading;

namespace SpeechAgent.Services
{
  public interface IUpdateService
  {
    event EventHandler<UpdateErrorEventArgs>? UpdateError;
     
    bool IsUpdateApplied { get; set; }
  }

  public class UpdateErrorEventArgs : EventArgs
  {
    public Exception Exception { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
  }

  public class UpdateService : IUpdateService
  {
    private const string UpdateAppliedFlag = "SPEECH_AGENT_UPDATE_APPLIED";

    public bool IsUpdateApplied
    {
      get =>
        Environment.GetEnvironmentVariable(UpdateAppliedFlag, EnvironmentVariableTarget.User)
        == "1";
      set
      {
        if (value)
          Environment.SetEnvironmentVariable(
            UpdateAppliedFlag,
            "1",
            EnvironmentVariableTarget.User
          );
        else
          Environment.SetEnvironmentVariable(
            UpdateAppliedFlag,
            null,
            EnvironmentVariableTarget.User
          );
      }
    }

    public event EventHandler<UpdateErrorEventArgs>? UpdateError;

    public UpdateService()
    {
      //_updateUrl = Environment.Is64BitProcess
      //  ? UpdateConfig.UpdateUrlX64
      //  : UpdateConfig.UpdateUrlX86;
    } 
  }
}
