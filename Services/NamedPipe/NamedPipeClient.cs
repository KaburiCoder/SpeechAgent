using SpeechAgent.Utils;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SpeechAgent.Services.NamedPipe
{
  public class NamedPipeClient
  {
    private readonly string _pipeName;
    private NamedPipeClientStream? _pipeStream;
    private StreamReader? _streamReader;
    private StreamWriter? _streamWriter;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isManuallyDisconnected = false;
    private bool _isReconnecting = false;
    private readonly int _reconnectIntervalMs = 5000;
    private int _connectAttemptCount = 0;
    public event EventHandler<string>? MessageReceived;
    public event EventHandler<Exception>? ConnectionError;
    public event EventHandler? Disconnected;
    public event EventHandler? Connected;

    public NamedPipeClient(string pipeName)
    {
      _pipeName = pipeName;
    }

    /// <summary>
    /// 파이프 서버에 연결을 시작합니다.
    /// </summary>
    public async Task ConnectAsync(int timeoutMs = 5000)
    {
      _isManuallyDisconnected = false;
      _isReconnecting = false;
      await TryConnectAsync(timeoutMs);
    }

    /// <summary>
    /// 파이프 서버 연결을 시도합니다.
    /// </summary>
    private async Task TryConnectAsync(int timeoutMs)
    {
      _connectAttemptCount++;
      int attempt = _connectAttemptCount;
      try
      {
        LogUtils.WriteLog(LogLevel.Debug, $"파이프 연결 시도 #{attempt} (PipeName=\\\\.\\pipe\\{_pipeName}, timeout={timeoutMs}ms)");
        CleanupConnection();

        _pipeStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await _pipeStream.ConnectAsync(timeoutMs);

        InitializeStreams();

        LogUtils.WriteLog(LogLevel.Info, $"파이프 서버에 연결되었습니다. PipeName={_pipeName}, attempt=#{attempt}");
        // 연결 성공 시 카운터 리셋 — 다음 끊김부터 새로 카운트
        _connectAttemptCount = 0;
        Connected?.Invoke(this, EventArgs.Empty);

        StartMessageReading();
      }
      catch (TimeoutException ex)
      {
        LogUtils.WriteLog(LogLevel.Warn, $"파이프 연결 타임아웃 #{attempt} (PipeName={_pipeName}, timeout={timeoutMs}ms). 서버가 떠있지 않을 가능성", ex);
        _isReconnecting = false;
        HandleConnectionError(ex);
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, $"파이프 연결 실패 #{attempt} (PipeName={_pipeName})", ex);
        _isReconnecting = false;
        HandleConnectionError(ex);
      }
    }

    /// <summary>
    /// 스트림을 초기화합니다.
    /// </summary>
    private void InitializeStreams()
    {
      if (_pipeStream == null)
        throw new InvalidOperationException("파이프 스트림이 초기화되지 않았습니다.");

      _streamReader = new StreamReader(_pipeStream, Encoding.UTF8);
      _streamWriter = new StreamWriter(_pipeStream, Encoding.UTF8) { AutoFlush = true };
    }

    /// <summary>
    /// 메시지 읽기를 시작합니다.
    /// </summary>
    private void StartMessageReading()
    {
      _cancellationTokenSource = new CancellationTokenSource();
      _ = Task.Run(() => ReadMessagesAsync(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// 연결 오류를 처리합니다.
    /// </summary>
    private void HandleConnectionError(Exception ex)
    {
      LogUtils.WriteLog(LogLevel.Error, "파이프 연결 오류 발생", ex);
      ConnectionError?.Invoke(this, ex);

      CleanupConnection();

      if (!_isManuallyDisconnected)
      {
        StartAutoReconnect();
      }
      else
      {
        LogUtils.WriteLog(LogLevel.Debug, "수동 종료 상태 — 자동 재연결 생략");
      }
    }

    /// <summary>
    /// 자동 재연결을 시작합니다.
    /// </summary>
    private void StartAutoReconnect()
    {
      if (_isReconnecting)
      {
        return;
      }

      _isReconnecting = true;
      _ = AutoReconnectAsync();
    }

    /// <summary>
    /// 자동 재연결을 수행합니다. 일정 간격으로 계속 시도합니다.
    /// </summary>
    private async Task AutoReconnectAsync()
    {
      try
      {
        await WaitBeforeReconnect();

        if (_isManuallyDisconnected)
          return;

        await TryConnectAsync(5000);
      }
      finally
      {
        _isReconnecting = false;
      }
    }

    /// <summary>
    /// 재연결 전 대기 시간을 가집니다.
    /// </summary>
    private async Task WaitBeforeReconnect()
    {
      LogUtils.WriteLog(LogLevel.Info, $"파이프 자동 재연결 대기 ({_reconnectIntervalMs}ms, 다음시도 #{_connectAttemptCount + 1})");
      await Task.Delay(_reconnectIntervalMs);
    }

    /// <summary>
    /// 메시지를 계속 읽고 수신 이벤트를 발생시킵니다.
    /// </summary>
    private async Task ReadMessagesAsync(CancellationToken cancellationToken)
    {
      try
      {
        while (IsConnectionValid(cancellationToken))
        {
          await ReadSingleMessage(cancellationToken);
        }
      }
      catch (OperationCanceledException)
      {
        LogUtils.WriteLog(LogLevel.Debug, "파이프 메시지 읽기가 취소되었습니다.");
      }
      catch (IOException ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "파이프 연결이 끊어졌습니다 (IO).", ex);
        ConnectionError?.Invoke(this, ex);
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "파이프 메시지 수신 중 예외", ex);
        ConnectionError?.Invoke(this, ex);
      }
      finally
      {
        Disconnected?.Invoke(this, default!);
        HandleConnectionClose();
      }
    }

    /// <summary>
    /// 연결이 유효한지 확인합니다.
    /// </summary>
    private bool IsConnectionValid(CancellationToken cancellationToken)
    {
      return !cancellationToken.IsCancellationRequested &&
             _streamReader != null &&
             _pipeStream?.IsConnected == true;
    }

    /// <summary>
    /// 단일 메시지를 읽습니다.
    /// </summary>
    private async Task ReadSingleMessage(CancellationToken cancellationToken)
    {
      string? message = await _streamReader!.ReadLineAsync(cancellationToken);

      if (message != null)
      {
        MessageReceived?.Invoke(this, message);
      }
    }

    /// <summary>
    /// 연결이 닫히면 정리 처리를 수행합니다.
    /// </summary>
    private void HandleConnectionClose()
    {
      if (_isManuallyDisconnected)
      {
        LogUtils.WriteLog(LogLevel.Info, "파이프 연결을 수동으로 종료했습니다.");
        CleanupConnection();
      }
      else
      {
        LogUtils.WriteLog(LogLevel.Info, "파이프 연결이 끊어졌습니다. 자동 재연결을 시도합니다.");
        CleanupConnection();
        StartAutoReconnect();
      }
    }

    /// <summary>
    /// 연결 리소스를 정리합니다.
    /// </summary>
    private void CleanupConnection()
    {
      try
      {
        _streamWriter?.Dispose();
        _streamReader?.Dispose();
        _pipeStream?.Dispose();
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Debug, "연결 정리 중 예외", ex);
      }
    }

    /// <summary>
    /// 문자열 메시지를 전송합니다.
    /// </summary>
    public async Task SendMessageAsync(string message)
    {
      try
      {
        ValidateConnection();
        await _streamWriter!.WriteLineAsync(message);
        await _streamWriter.FlushAsync();
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "파이프 메시지 송신 실패", ex);
        ConnectionError?.Invoke(this, ex);
        throw;
      }
    }

    /// <summary>
    /// 제네릭 객체를 JSON으로 직렬화하여 전송합니다.
    /// </summary>
    public async Task SendAsync<T>(T message)
    {
      try
      {
        ValidateConnection();
        string jsonMessage = JsonSerializer.Serialize(message, JsonUtils.DefaultOptions);
        await _streamWriter!.WriteLineAsync(jsonMessage);
        await _streamWriter.FlushAsync();
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "파이프 메시지 송신 실패(JSON)", ex);
        ConnectionError?.Invoke(this, ex);
        throw;
      }
    }

    /// <summary>
    /// 연결 상태를 확인합니다.
    /// </summary>
    private void ValidateConnection()
    {
      if (_streamWriter == null || _pipeStream == null || !_pipeStream.IsConnected)
      {
        throw new InvalidOperationException("파이프 연결이 유효하지 않습니다.");
      }
    }

    /// <summary>
    /// 파이프 연결을 수동으로 종료합니다.
    /// </summary>
    public void Disconnect()
    {
      _isManuallyDisconnected = true;
      _isReconnecting = false;

      try
      {
        _cancellationTokenSource?.Cancel();
        CleanupConnection();

        LogUtils.WriteLog(LogLevel.Info, "파이프 연결을 수동으로 종료했습니다.");
        Disconnected?.Invoke(this, EventArgs.Empty);
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, "파이프 연결 종료 중 예외", ex);
      }
    }

    /// <summary>
    /// 현재 연결 상태를 반환합니다.
    /// </summary>
    public bool IsConnected => _pipeStream?.IsConnected ?? false;
  }
}
