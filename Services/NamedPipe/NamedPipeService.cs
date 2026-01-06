using SpeechAgent.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeechAgent.Services.NamedPipe
{
  public interface INamedPipeService
  {
    event EventHandler<string>? MessageReceived;
    event EventHandler<Exception>? ConnectionError;
    event EventHandler? Connected;
    event EventHandler? Disconnected;

    Task ConnectAsync(int timeoutMs = 5000);
    Task SendMessageAsync(string message);
    Task SendAsync(NamedPipeData data);
    void Disconnect();
    bool IsConnected { get; }
  }

  public class NamedPipeService : INamedPipeService
  {
    private readonly NamedPipeClient _pipeClient;
    private const string PIPE_NAME = "voice-medic-pipe";

    public event EventHandler<string>? MessageReceived;
    public event EventHandler<Exception>? ConnectionError;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public NamedPipeService()
    {
      _pipeClient = new NamedPipeClient(PIPE_NAME);
      _pipeClient.MessageReceived += (s, message) => MessageReceived?.Invoke(this, message);
      _pipeClient.ConnectionError += (s, error) => ConnectionError?.Invoke(this, error);
      _pipeClient.Connected += (s, e) => Connected?.Invoke(this, e);
      _pipeClient.Disconnected += (s, e) => Disconnected?.Invoke(this, e);
    }

    public async Task ConnectAsync(int timeoutMs = 5000)
    {
      try
      {
        await _pipeClient.ConnectAsync(timeoutMs);
      }
      catch
      {
        throw;
      }
    }

    public async Task SendMessageAsync(string message)
    {
      await _pipeClient.SendMessageAsync(message);
    } 

    public void Disconnect()
    {
      _pipeClient.Disconnect();
    }

    public async Task SendAsync(NamedPipeData data)
    {
      await _pipeClient.SendAsync(data);
    }

    public bool IsConnected => _pipeClient.IsConnected;
  }
}
