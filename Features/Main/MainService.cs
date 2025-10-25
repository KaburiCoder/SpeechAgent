using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using SpeechAgent.Services.MedicSIO;
using SpeechAgent.Services.MedicSIO.Dto;
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
    private readonly IAutomationControlSearchService _automationControlSearchService;
    private readonly IMedicSIOService _medicSIOService;
    private readonly DispatcherTimer _timer;
    private PatientInfo _patientInfo = new("", "", DateTime.MinValue);

    public MainService(
      IAutomationControlSearchService automationControlSearchService,
      IMedicSIOService medicSIOService)
    {
      _automationControlSearchService = automationControlSearchService;
      _medicSIOService = medicSIOService;
      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1); // 1초 간격
      _timer.Tick += Timer_Tick;

      WeakReferenceMessenger.Default.Register<LocalSettingsChangedMessage>(this, (r, m) =>
      {
        _automationControlSearchService.Clear();
        bool isNoneTargetApp = string.IsNullOrWhiteSpace(m.Value.Settings.TargetAppName);
        if (isNoneTargetApp)
          _timer.Stop();
        else
          _timer.Start();
      });
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
      // UI Automation 사용
      var automationControls = _automationControlSearchService.FindChartAndNameControls();
      string chart = automationControls?.ChartTextBox?.Text ?? "";
      string suname = automationControls?.NameTextBox?.Text ?? "";

      var previousPatientInfo = _patientInfo;
      _patientInfo = _patientInfo with { Chart = chart, Name = suname };
      if (previousPatientInfo != _patientInfo)
      {
        var res = await _medicSIOService.SendPatientInfo(new PatientInfoDto
        {
          Chart = chart,
          Name = suname
        });
        WeakReferenceMessenger.Default.Send(new PatientInfoUpdatedMessage(new PatientInfo(chart, suname, DateTime.Now)));
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
