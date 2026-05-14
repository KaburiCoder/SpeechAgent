using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// UI 스레드에서 일정 주기로 "살아있다" 로그를 기록합니다.
  /// UI Dispatcher가 hang되면 heartbeat가 멈추므로 마지막 heartbeat 시각으로 hang 시점을 추정할 수 있습니다.
  /// </summary>
  public sealed class HeartbeatLogger : IDisposable
  {
    private readonly DispatcherTimer _timer;
    private int _count;

    public HeartbeatLogger(TimeSpan interval)
    {
      _timer = new DispatcherTimer { Interval = interval };
      _timer.Tick += OnTick;
    }

    public void Start()
    {
      _timer.Start();
      LogUtils.WriteLog(LogLevel.Debug, $"UI heartbeat 타이머 시작 ({_timer.Interval.TotalSeconds:N0}초 주기)");
    }

    private void OnTick(object? sender, EventArgs e)
    {
      try
      {
        _count++;
        using var proc = Process.GetCurrentProcess();
        long wsMB = proc.WorkingSet64 / (1024 * 1024);
        long managedMB = GC.GetTotalMemory(false) / (1024 * 1024);
        int threads = proc.Threads.Count;
        int handles = proc.HandleCount;
        var uptime = DateTime.Now - proc.StartTime;

        LogUtils.WriteLog(
          LogLevel.Info,
          $"heartbeat #{_count} ws={wsMB}MB managed={managedMB}MB " +
          $"threads={threads} handles={handles} uptime={uptime:hh\\:mm\\:ss}");
      }
      catch
      {
        // heartbeat 자체가 실패해도 앱 동작에 영향 없게 무시
      }
    }

    public void Dispose()
    {
      _timer.Stop();
      _timer.Tick -= OnTick;
    }
  }
}
