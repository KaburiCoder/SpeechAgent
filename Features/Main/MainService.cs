using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using SpeechAgent.Services.MedicSIO;
using SpeechAgent.Services.MedicSIO.Dto;
using System.Diagnostics;
using System.Windows.Threading;

namespace SpeechAgent.Features.Main
{
  public interface IMainService
  {
    Task StartReadChartTimer();
    Task StopReadChartTimer();
  }

  public class MainService : IMainService
  {
    private readonly IControlSearchService _controlSearchService;
    private readonly IMedicSIOService _medicSIOService;
    private readonly DispatcherTimer _timer;
    private PatientInfo _patientInfo = new("", "", DateTime.MinValue);

    public MainService(
      IControlSearchService controlSearchService,
      IMedicSIOService medicSIOService)
    {
      _controlSearchService = controlSearchService;
      _medicSIOService = medicSIOService; 
      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1); // 1초 간격
      _timer.Tick += Timer_Tick;
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
      var appControls = _controlSearchService.FindChartAndNameControls();
      var chart = appControls?.ChartTextBox?.Text ?? "";
      var suname = appControls?.NameTextBox?.Text ?? "";

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

    public async Task StartReadChartTimer()
    {
      await _medicSIOService.Connect();
      _timer.Start();
    }

    public async Task StopReadChartTimer()
    {
      await _medicSIOService.DisConnect();
      _timer.Stop();
    }
  }
}
