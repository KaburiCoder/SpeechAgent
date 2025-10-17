using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using System.Diagnostics;
using System.Windows.Threading;

namespace SpeechAgent.Features.Main
{
  public interface IMainService
  {
    void StartReadChartTimer();
    void StopReadChartTimer();
  }

  public class MainService : IMainService
  {
    private readonly IControlSearchService _controlSearchService;
    private readonly DispatcherTimer _timer;
    private PatientInfo _patientInfo = new("", "");

    public MainService(IControlSearchService controlSearchService)
    {
      _controlSearchService = controlSearchService;

      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1); // 1초 간격
      _timer.Tick += Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
      var appControls = _controlSearchService.FindChartAndNameControls();
      var chart = appControls?.ChartTextBox?.Text ?? "";
      var suname = appControls?.NameTextBox?.Text ?? "";

      var previousPatientInfo = _patientInfo;
      _patientInfo = _patientInfo with { Chart = chart, Name = suname };
      if (previousPatientInfo != _patientInfo)
      {
        WeakReferenceMessenger.Default.Send(new PatientInfoUpdatedMessage(_patientInfo));
      }
    }

    public void StartReadChartTimer()
    {
      _timer.Start();

    }

    public void StopReadChartTimer()
    {
      _timer.Stop();
    }
  }
}
