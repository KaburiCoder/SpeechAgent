using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
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
    ILlmApi llmApi,
    IClickSoftControlSearchService _clickSoftControlSearchService
  ) : IPatientSearchService
  {
    private AutomationAppControls _appControls = new();
    private PatientImageResult _previousImageResult = new();
    private int _nullCount = 0; // 컨트롤을 찾지 못한 연속 횟수 (10회 이상이면 초기화)
    private const int MaxNullCount = 10; // 초기화 기준 횟수

    LocalSettings Settings => _settingsService.Settings;
    bool IsCustom =>
      Settings.TargetAppName == AppKey.CustomUser
      || Settings.TargetAppName == AppKey.CustomUserWinApi
      || Settings.TargetAppName == AppKey.CustomUserImage;

    bool IsCustomUserWithoutImage =>
      Settings.TargetAppName == AppKey.CustomUser
      || Settings.TargetAppName == AppKey.CustomUserWinApi;

    /// <summary>
    /// 컨트롤 검색 성공 시 _nullCount를 초기화합니다.
    /// </summary>
    private void ResetNullCount()
    {
      _nullCount = 0;
    }

    /// <summary>
    /// 컨트롤 검색 실패 시 _nullCount를 증가시키고, 일정 횟수 이상이면 상태를 초기화합니다.
    /// </summary>
    private void IncrementAndCheckNullCount()
    {
      _nullCount++;
      if (_nullCount >= MaxNullCount)
      {
        Clear();
      }
    }

    private bool FindWindowByTitle(out bool isNewCreated)
    {
      isNewCreated = false;
      if (!_searcher.IsWindowValid())
      {
        if (IsCustom)
        {
          if (!_searcher.FindWindowByTitle(title => title.Contains(Settings.CustomExeTitle)))
            return false;
        }
        else
        {
          switch (Settings.TargetAppName)
          {
            case AppKey.USarang:
              if (!_searcher.FindWindowByTitles("진료실", "툴버전"))
                return false;
              break;
            case AppKey.Brain:
              if (!_searcher.FindWindowByTitles("진료실", "ver"))
                return false;
              break;
            case AppKey.DRChart:
            case AppKey.BitUChart:
              if (!_searcher.FindWindowByTitles("진료실"))
                return false;
              break;
            case AppKey.Doctors:
              if (!_searcher.FindWindowByTitles("진료실", "◎"))
                return false;
              break;
            default:
              if (!_searcher.FindWindowByTitles("진료실["))
                return false;
              break;
          }
        }
        isNewCreated = true;
      }

      return true;
    }

    private AutomationAppControls? GetAppControls(List<AutomationControlInfo> controls)
    {
      AutomationAppControls? result = null;

      switch (Settings.TargetAppName)
      {
        case AppKey.Brain:
          result = FindBrainControls(controls);
          break;
        case AppKey.USarang:
          result = FindUSarangControls(controls);
          break;
        case AppKey.BitUChart:
          result = FindControls(
            controls,
            chartInfo: new() { Index = 1 },
            nameInfo: new() { ControlType = "ControlType.Text", Index = 8 }
          );
          break;
        case AppKey.Doctors:
          result = FindControls(
            controls,
            chartInfo: new()
            {
              ControlType = "ControlType.Title",
              Index = 0,
              Regex = new() { GroupIndex = 1, Pattern = @"◎\s+(\S+)\s+([^\s◎]+)" },
            },
            nameInfo: new()
            {
              ControlType = "ControlType.Title",
              Index = 0,
              Regex = new() { GroupIndex = 2, Pattern = @"◎\s+(\S+)\s+([^\s◎]+)" },
            }
          );
          break;
        case AppKey.DRChart:
          result = FindControls(
            controls,
            chartInfo: new()
            {
              ControlType = "ControlType.TitleBar",
              Index = 0,
              Regex = new() { GroupIndex = 2, Pattern = @"^(.?)((\d+)).*" },
            },
            nameInfo: new()
            {
              ControlType = "ControlType.TitleBar",
              Index = 0,
              Regex = new() { GroupIndex = 1, Pattern = @"^(.?)((\d+)).*" },
            }
          );
          break;
        case AppKey.CustomUser:
          result = FindCustomControls(controls, Settings);
          break;
        case AppKey.ClickSoft:
          result = FindDefaultControls(controls);
          break;
      }

      if (result != null && result.ChartTextBox != null)
      {
        _appControls.SetControls(result.ChartTextBox, result.NameTextBox);
        return _appControls;
      }

      return null;
    }

    private AutomationAppControls? FindCustomControls(
      List<AutomationControlInfo> controls,
      LocalSettings settings
    )
    {
      AutomationControlInfo? chartEdit = null;
      AutomationControlInfo? nameEdit = null;

      if (
        !string.IsNullOrEmpty(settings.CustomChartControlType)
        && !string.IsNullOrEmpty(settings.CustomChartIndex)
      )
      {
        var chartControls = controls
          .Where(c => c.ControlType == settings.CustomChartControlType)
          .ToList();
        if (
          int.TryParse(settings.CustomChartIndex, out int chartIndex)
          && chartIndex < chartControls.Count
        )
        {
          chartEdit = chartControls[chartIndex];
        }
      }

      if (
        !string.IsNullOrEmpty(settings.CustomNameControlType)
        && !string.IsNullOrEmpty(settings.CustomNameIndex)
      )
      {
        var nameControls = controls
          .Where(c => c.ControlType == settings.CustomNameControlType)
          .ToList();
        if (
          int.TryParse(settings.CustomNameIndex, out int nameIndex)
          && nameIndex < nameControls.Count
        )
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
        chartTextBox = controls.FirstOrDefault(c =>
          c.ControlType == "ControlType.Edit" && c.Index == 1
        );
        nameTextBox = controls.FirstOrDefault(c =>
          c.ControlType == "ControlType.Edit" && c.Index == 7
        );
        appControls.SetControls(chartTextBox, nameTextBox);
        return appControls;
      }
    }

    private AutomationAppControls? FindBrainControls(List<AutomationControlInfo> controls)
    {
      var editControls = controls.Where(c => c.ControlType == "ControlType.Edit").ToList();
      var chartControl = editControls.First();
      var nameControl = chartControl;

      if (chartControl != null && nameControl != null)
      {
        var appControls = new AutomationAppControls();
        appControls.SetControls(chartControl, nameControl);
        return appControls;
      }

      return null;
    }

    private AutomationAppControls? FindControls(
      List<AutomationControlInfo> controls,
      FindControlInfo chartInfo,
      FindControlInfo nameInfo
    )
    {
      var chartControl = controls.FirstOrDefault(c =>
        c.ControlType == chartInfo.ControlType && c.Index == chartInfo.Index
      );
      var nameControl = controls.FirstOrDefault(c =>
        c.ControlType == nameInfo.ControlType && c.Index == nameInfo.Index
      );

      if (chartControl != null && nameControl != null)
      {
        // 정규식 정의 되어있는 경우 전달
        chartControl.Regex = chartInfo.Regex;
        nameControl.Regex = nameInfo.Regex;
        var appControls = new AutomationAppControls();
        appControls.SetControls(chartControl, nameControl);
        return appControls;
      }

      return null;
    }

    private AutomationAppControls? FindUSarangControls(List<AutomationControlInfo> controls)
    {
      // ControlType.Edit인 컨트롤들을 필터링
      var editControls = controls.Where(c => c.ControlType == "ControlType.Edit").ToList();

      if (editControls.Count < 2)
        return null;

      // 한 라인에 가장 많은 컨트롤이 있는 그룹 찾기
      var maxGroup = editControls
        .GroupBy(x => x.RectTop)
        .OrderByDescending(g => g.Count())
        .FirstOrDefault();

      var chartControl = maxGroup?.First();

      // 나머지 컨트롤 중에서 X값이 가장 가까운 컨트롤을 찾기
      var nameControl = maxGroup
        ?.Where(c => c != chartControl)
        .OrderBy(c =>
          Math.Abs(c.BoundingRectangle.Left - chartControl?.BoundingRectangle.Left ?? 0)
        )
        .First();
      chartControl = _searcher.CreateControlInfo(chartControl?.Element);

      if (chartControl == null || nameControl == null)
        return null;

      //// 테스트
      //chartControl = editControls[2];
      //nameControl = editControls[3];

      // OCR 수행
      try
      {
        IntPtr hWnd = _searcher.GetWindowHandle();
        if (hWnd != IntPtr.Zero)
        {
          // 윌도우의 절대 좌표 얻기
          Vanara.PInvoke.User32.GetWindowRect(new Vanara.PInvoke.HWND(hWnd), out var windowRect);

          // 절대 좌표를 윈도우 상대 좌표로 변환 (DPI 스케일 적용)
          var dpiScale = DpiUtils.GetDpiScale(hWnd);
          var chartRectRelative = DpiUtils.ConvertToWindowRelativeRect(
            chartControl.BoundingRectangle,
            windowRect,
            dpiScale
          );

          var chartImg = _windowCaptureService.CaptureWindow(hWnd, chartRectRelative);

          //chartImg?.SaveBitmapSourceToFile(
          //  System.IO.Path.Combine(
          //    AppDomain.CurrentDomain.BaseDirectory,
          //    "Log",
          //    "USarang_Chart_OCR.png"
          //  )
          //);

          // 이전 이미지와 유사하면 캐시된 정보 사용
          if (OpenCvUtils.AreImagesSimilar(_previousImageResult.BitmapSource, chartImg, 1))
            return _appControls;

          _previousImageResult.BitmapSource = chartImg;

          // BitmapSource를 Bitmap으로 변환 후 OCR 수행
          if (chartImg != null)
          {
            string? previousChart = _appControls.ChartTextBox?.Text;
            chartControl.Text = chartImg.OcrUSarangChart().Trim();
            if (previousChart != chartControl.Text)
            {
              nameControl.Text = string.IsNullOrWhiteSpace(chartControl.Text)
                ? ""
                : _searcher.GetControlText(nameControl.Element);
            }
          }
          else
          {
            _previousImageResult.Clear();
            return null;
          }
        }
      }
      catch (Exception ex)
      {
        var infoText = $"Error: {ex.ToString()}";
        var filePath = System.IO.Path.Combine(
          System.AppDomain.CurrentDomain.BaseDirectory,
          "USarangControls.txt"
        );
        LogUtils.WriteLog(LogLevel.Error, $"[USarang] {infoText}");
      }

      if (chartControl != null && nameControl != null)
      {
        var appControls = new AutomationAppControls();
        appControls.SetControls(chartControl, nameControl);
        return appControls;
      }

      return null;
    }

    public async Task<PatientInfo> FindPatientInfo()
    {
      // ClickSoft 일때 Win32 API 우선 사용
      if (Settings.TargetAppName == AppKey.ClickSoft)
      {
        var win32Result = _clickSoftControlSearchService.FindControls();
        if (
          win32Result != null
          && win32Result.ChartTextBox != null
          && win32Result.NameTextBox != null
        )
        {
          _appControls.SetControls(win32Result.ChartTextBox, win32Result.NameTextBox);
          ResetNullCount();
          return CreatePatientInfo();
        }
        // Win32로 못 찾으면 _nullCount 증가
        IncrementAndCheckNullCount();
        // Win32로 못 찾으면 Automation으로 계속 시도
      }

      // 윈도우 타이틀로 핸들 찾기
      if (!FindWindowByTitle(out bool isNewCreated))
      {
        _searcher.ClearFoundControls();
        _appControls.ClearControls();
        _previousImageResult.Clear();
        return new PatientInfo();
      }

      if (_settingsService.UseCustomUserImage)
      {
        IntPtr hWnd = _searcher.GetWindowHandle();
        PatientInfo patientInfo = new PatientInfo();

        if (hWnd == IntPtr.Zero)
          return patientInfo;

        BitmapSource? bitmapSource = _windowCaptureService.CaptureWindow(
          hWnd,
          captureRect: _settingsService.Settings.ParseCustomImageRect()
        );

        //bitmapSource?.SaveBitmapSourceToFile(
        //  System.IO.Path.Combine(
        //    AppDomain.CurrentDomain.BaseDirectory,
        //    "Log",
        //    "CustomUserImage_Capture.png"
        //  )
        //);
        if (bitmapSource == null)
        {
          _previousImageResult.Clear();
        }
        // 이전 이미지와 유사하면 캐시된 정보 사용
        else if (
          _previousImageResult.BitmapSource == null
          || !OpenCvUtils.AreImagesSimilar(_previousImageResult.BitmapSource, bitmapSource, 1)
        )
        {
          var patientInfoDto = await llmApi.GetPatientInfoByImage(bitmapSource.ToDataUrl());
          patientInfo = new PatientInfo(patientInfoDto.Chart, patientInfoDto.Name);
          _previousImageResult.SetResult(patientInfo, bitmapSource);
        }

        return _previousImageResult.PatientInfo ?? patientInfo;
      }
      else
      {
        if (_settingsService.Settings.TargetAppName != AppKey.USarang) // 의사랑은 OCR로 처리하므로 캐시 제외
        {
          // 기존 윈도우면 캐시된 컨트롤 사용

          if (!isNewCreated)
          {
            if (_appControls.ChartTextBox != null && _appControls.NameTextBox != null)
            {
              ResetNullCount();
              var chartInfo = _searcher.CreateControlInfo(_appControls.ChartTextBox.Element);
              var nameInfo = _searcher.CreateControlInfo(_appControls.NameTextBox.Element);
              _appControls.ChartTextBox.Text = chartInfo?.Text ?? "";
              _appControls.NameTextBox.Text = nameInfo?.Text ?? "";
              return CreatePatientInfo();
            }
            else
            {
              IncrementAndCheckNullCount();
            }
          }
        }

        var controls =
          _searcher.FoundControls.Count != 0 ? _searcher.FoundControls : _searcher.SearchControls();

        // 차트, 수진자명 컨트롤 찾아서 전달
        GetAppControls(controls);

        return CreatePatientInfo();
      }
    }

    private PatientInfo CreatePatientInfo()
    {
      var chartTextBox = _appControls.ChartTextBox;
      var nameTextBox = _appControls.NameTextBox;

      if (Settings.TargetAppName == AppKey.Brain)
      {
        var splitResults = chartTextBox?.Text?.Split(" ", 2);
        if (splitResults != null && splitResults.Length >= 2)
          return new PatientInfo { Chart = splitResults[0].Trim(), Name = splitResults[1].Trim() };
      }

      string chartPattern = "",
        namePattern = ""; // Settings.CustomChartRegex ?? chartTextBox?.Regex?.Pattern ?? "";
      int chartRegexIndex = 0,
        nameRegexIndex = 0;

      if (IsCustomUserWithoutImage)
      {
        chartPattern = Settings.CustomChartRegex;
        namePattern = Settings.CustomNameRegex;
        chartRegexIndex = Settings.CustomChartRegexIndex;
        nameRegexIndex = Settings.CustomNameRegexIndex;
      }
      else
      {
        chartPattern = chartTextBox?.Regex?.Pattern ?? "";
        namePattern = nameTextBox?.Regex?.Pattern ?? "";
        chartRegexIndex = chartTextBox?.Regex?.GroupIndex ?? 0;
        nameRegexIndex = nameTextBox?.Regex?.GroupIndex ?? 0;
      }

      string chart = chartTextBox?.Text?.GetRegexString(chartPattern, chartRegexIndex) ?? "";
      string name = nameTextBox?.Text?.GetRegexString(namePattern, nameRegexIndex) ?? "";

      return new PatientInfo { Chart = chart, Name = name };
    }

    public void Clear()
    {
      _clickSoftControlSearchService.Clear();
      _searcher.ClearFoundControls();
      _appControls.ClearControls();
      _previousImageResult.Clear();
      _nullCount = 0;
    }
  }
}
