using SpeechAgent.Constants;
using SpeechAgent.Database.Schemas;
using SpeechAgent.Database.Utils;
using SpeechAgent.Features.Settings;
using SpeechAgent.Features.Settings.FindWin.Services;
using SpeechAgent.Models;
using SpeechAgent.Services.Api;
using SpeechAgent.Utils;
using SpeechAgent.Utils.Automation;
using SpeechAgent.Utils.Converters;
using System.Windows.Media.Imaging;

namespace SpeechAgent.Services
{
  public interface IPatientSearchService
  {
    void Clear();
    Task<PatientInfo> FindPatientInfo();
  }

  public class PatientSearchService(
    IAutomationControlSearcher _searcher,
    ISettingsService _settingsService,
    IWindowCaptureService _windowCaptureService,
    ILlmApi llmApi) : IPatientSearchService
  {
    private AutomationAppControls _appControls = new();
    private PatientImageResult _previousImageResult = new();

    private bool FindWindowByTitle(out bool isNewCreated)
    {
      var settings = _settingsService.Settings;

      isNewCreated = false;
      if (!_searcher.IsWindowValid())
      {
        _searcher.ClearFoundControls();

        bool isCustom = settings.TargetAppName == AppKey.CustomUser || settings.TargetAppName == AppKey.CustomUserImage;

        if (isCustom)
        {
          if (!_searcher.FindWindowByTitle(title => title.Contains(settings.CustomExeTitle)))
            return false;
        }
        else
        {
          if (!_searcher.FindWindowByTitle(title => title.Contains("진료실[")))
            return false;
        }
        isNewCreated = true;
      }

      return true;
    }

    private AutomationAppControls? GetAppControls(List<AutomationControlInfo> controls)
    {
      var settings = _settingsService.Settings;

      AutomationAppControls? result = null;
      if (settings.TargetAppName == AppKey.CustomUser)
      {
        result = FindCustomControls(controls, settings);
      }
      else if (settings.TargetAppName == AppKey.ClickSoft)
      {
        result = FindDefaultControls(controls);
      }

      if (result != null && result.ChartTextBox != null)
      {
        _appControls.SetControls(result.ChartTextBox, result.NameTextBox);
        return _appControls;
      }

      return null;
    }

    private AutomationAppControls? FindCustomControls(List<AutomationControlInfo> controls, LocalSettings settings)
    {
      AutomationControlInfo? chartEdit = null;
      AutomationControlInfo? nameEdit = null;

      if (!string.IsNullOrEmpty(settings.CustomChartControlType) && !string.IsNullOrEmpty(settings.CustomChartIndex))
      {
        var chartControls = controls.Where(c => c.ControlType == settings.CustomChartControlType).ToList();
        if (int.TryParse(settings.CustomChartIndex, out int chartIndex) && chartIndex < chartControls.Count)
        {
          chartEdit = chartControls[chartIndex];
        }
      }

      if (!string.IsNullOrEmpty(settings.CustomNameControlType) && !string.IsNullOrEmpty(settings.CustomNameIndex))
      {
        var nameControls = controls.Where(c => c.ControlType == settings.CustomNameControlType).ToList();
        if (int.TryParse(settings.CustomNameIndex, out int nameIndex) && nameIndex < nameControls.Count)
        {
          nameEdit = nameControls[nameIndex];
        }
      }

      if (chartEdit != null)
      {
        var appControls = new AutomationAppControls();
        appControls.SetControls(chartEdit, nameEdit);
        return appControls;
      }

      return null;
    }

    private AutomationAppControls? FindDefaultControls(List<AutomationControlInfo> controls)
    {
      var chartTextBox = controls.FirstOrDefault(c => c.AutomationId.ToLower() == "txt_chart");
      var nameTextBox = controls.FirstOrDefault(c => c.AutomationId.ToLower() == "txt_suname");

      if (chartTextBox != null && nameTextBox != null)
      {
        // eClick
        var appControls = new AutomationAppControls();
        appControls.SetControls(chartTextBox, nameTextBox);
        return appControls;
      }
      else
      {
        // newClick
        var appControls = new AutomationAppControls();
        chartTextBox = controls.FirstOrDefault(c => c.ClassName == "Edit" && c.Index == 1);
        nameTextBox = controls.FirstOrDefault(c => c.ClassName == "ThunderRT6TextBox" && c.Index == 0);
        appControls.SetControls(chartTextBox, nameTextBox);
        return appControls;
      }
    }

    public async Task<PatientInfo> FindPatientInfo()
    {
      // 윈도우 타이틀로 핸들 찾기   
      if (!FindWindowByTitle(out bool isNewCreated))
      {
        _appControls.ClearControls();
        _previousImageResult.Clear();
        return new PatientInfo();
      }

      if (_settingsService.UseCustomUserImage)
      {
        IntPtr hWnd = _searcher.GetWindowHandle();
        PatientInfo patientInfo = new PatientInfo();

        if (hWnd == IntPtr.Zero) return patientInfo;

        BitmapSource? bitmapSource = _windowCaptureService.CaptureWindow(hWnd, captureRect: _settingsService.Settings.ParseCustomImageRect());
        if (bitmapSource == null)
        {
          _previousImageResult.Clear();
        }
        // 이전 이미지와 유사하면 캐시된 정보 사용
        else if (_previousImageResult.BitmapSource == null ||
          !OpenCvUtils.AreImagesSimilar(_previousImageResult.BitmapSource, bitmapSource, 1))
        {
          var patientInfoDto = await llmApi.GetPatientInfoByImage(bitmapSource.ToDataUrl());
          patientInfo = new PatientInfo(patientInfoDto.Chart, patientInfoDto.Name);
          _previousImageResult.SetResult(patientInfo, bitmapSource);
        }

        return _previousImageResult.PatientInfo ?? patientInfo;
      }
      else
      {
        // 기존 윈도우면 캐시된 컨트롤 사용
        if (!isNewCreated && _appControls.ChartTextBox != null && _appControls.NameTextBox != null)
        {
          _appControls.ChartTextBox.Text = _searcher.GetControlText(_appControls.ChartTextBox.Element);
          _appControls.NameTextBox.Text = _searcher.GetControlText(_appControls.NameTextBox.Element);
          return CreatePatientInfo();
        }
        var controls = _searcher.FoundControls.Count != 0
          ? _searcher.FoundControls
          : _searcher.SearchControls();

        // 차트, 수진자명 컨트롤 찾아서 전달
        GetAppControls(controls);

        return CreatePatientInfo();
      }
    }

    private PatientInfo CreatePatientInfo()
    {
      return new PatientInfo
      {
        Chart = _appControls.ChartTextBox?.Text ?? "",
        Name = _appControls.NameTextBox?.Text ?? "",
      };
    }
    public void Clear()
    {
      _searcher.ClearFoundControls();
      _appControls.ClearControls();
    }
  }

  public class AutomationAppControls
  {
    public AutomationControlInfo? ChartTextBox { get; private set; }
    public AutomationControlInfo? NameTextBox { get; private set; }

    public void SetControls(AutomationControlInfo? chartTextBox, AutomationControlInfo? nameTextBox)
    {
      ChartTextBox = chartTextBox;
      NameTextBox = nameTextBox;
    }

    public void ClearControls()
    {
      ChartTextBox = null;
      NameTextBox = null;
    }
  }

  public class PatientImageResult
  {
    public PatientInfo? PatientInfo { get; private set; }
    public BitmapSource? BitmapSource { get; private set; }

    public void SetResult(PatientInfo? patientInfo, BitmapSource? bitmapSource)
    {
      PatientInfo = patientInfo;
      BitmapSource = bitmapSource;
    }

    public void Clear()
    {
      PatientInfo = null;
      BitmapSource = null;
    }
  }
}
