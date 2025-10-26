using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Constants;
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
    private readonly IPatientSearchService _patientSearchService;
    private readonly IMedicSIOService _medicSIOService;
    private readonly ISettingsService _settingsService;
    private readonly DispatcherTimer _timer;
    private PatientInfo _patientInfo = new("", "", DateTime.MinValue);

    public MainService(
      IPatientSearchService patientSearchService,
      IMedicSIOService medicSIOService,
      ISettingsService settingsService)
    {
      _patientSearchService = patientSearchService;
      _medicSIOService = medicSIOService;
      this._settingsService = settingsService;
      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1); // 1초 간격
      _timer.Tick += Timer_Tick;

      WeakReferenceMessenger.Default.Register<LocalSettingsChangedMessage>(this, (r, m) =>
      {
        _patientSearchService.Clear();
        bool isNoneTargetApp = string.IsNullOrWhiteSpace(m.Value.Settings.TargetAppName);
        if (isNoneTargetApp)
          _timer.Stop();
        else
        {
          StartReadChartTimer();
        }
      });
    }


    private async void Timer_Tick(object? sender, EventArgs e)
    {
      // UI Automation 사용      
      var patientInfo = await _patientSearchService.FindPatientInfo();
      var previousPatientInfo = _patientInfo;
      _patientInfo = patientInfo;
      if (previousPatientInfo.Chart != _patientInfo.Chart)
      {
        var res = await _medicSIOService.SendPatientInfo(new PatientInfoDto
        {
          Chart = _patientInfo.Chart,
          Name = _patientInfo.Name
        });
        WeakReferenceMessenger.Default.Send(new PatientInfoUpdatedMessage(_patientInfo));
      }
    }

    public void StartReadChartTimer()
    {
      int intervalSec = (_settingsService.Settings.TargetAppName == AppKey.CustomUserImage) ? 3 : 1;

      _timer.Interval = TimeSpan.FromSeconds(intervalSec);
      _timer.Start();
    }

    public void StopReadChartTimer()
    {
      _timer.Stop();
    }
  }
}
