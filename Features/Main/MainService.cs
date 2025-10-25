using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Features.Settings;
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
    private readonly IControlSearchService _controlSearchService;
    private readonly IAutomationControlSearchService _automationControlSearchService;
    private readonly ISettingsService _settingsService;
    private readonly IMedicSIOService _medicSIOService;
    private readonly DispatcherTimer _timer;
    private PatientInfo _patientInfo = new("", "", DateTime.MinValue);

    public MainService(
      IControlSearchService controlSearchService,
    IAutomationControlSearchService automationControlSearchService,
      ISettingsService settingsService,
      IMedicSIOService medicSIOService)
    {
      _controlSearchService = controlSearchService;
      _automationControlSearchService = automationControlSearchService;
      _settingsService = settingsService;
      _medicSIOService = medicSIOService;
      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1); // 1초 간격
      _timer.Tick += Timer_Tick;

      WeakReferenceMessenger.Default.Register<LocalSettingsChangedMessage>(this, (r, m) =>
      {
        bool isNoneTargetApp = string.IsNullOrWhiteSpace(m.Value.Settings.TargetAppName);
        if (isNoneTargetApp)
       _timer.Stop();
    else
 _timer.Start();
    });
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
      var settings = _settingsService.Settings;
      string chart = "";
      string suname = "";

    if (settings.UseAutomation)
      {
        // UI Automation 사용
        var automationControls = _automationControlSearchService.FindChartAndNameControls();
        chart = automationControls?.ChartTextBox?.Text ?? "";
  suname = automationControls?.NameTextBox?.Text ?? "";
      }
      else
      {
      // Legacy WinAPI 사용
        var appControls = _controlSearchService.FindChartAndNameControls();
        chart = appControls?.ChartTextBox?.Text ?? "";
        suname = appControls?.NameTextBox?.Text ?? "";
      }

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
