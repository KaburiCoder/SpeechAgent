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
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

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
    IClickSoftControlSearchService _clickSoftControlSearchService) : IPatientSearchService
  {
    private AutomationAppControls _appControls = new();
    private PatientImageResult _previousImageResult = new();
    private int _nullCount = 0; // ��Ʈ���� ã�� ���� ���� Ƚ�� (10ȸ �̻��̸� �ʱ�ȭ)
    private const int MaxNullCount = 10; // �ʱ�ȭ ���� Ƚ��

    LocalSettings Settings => _settingsService.Settings;
    bool IsCustom => Settings.TargetAppName == AppKey.CustomUser ||
      Settings.TargetAppName == AppKey.CustomUserWinApi ||
      Settings.TargetAppName == AppKey.CustomUserImage;

    bool IsCustomNotImage => Settings.TargetAppName == AppKey.CustomUser || Settings.TargetAppName == AppKey.CustomUserWinApi;

    /// <summary>
    /// ��Ʈ�� �˻� ���� �� _nullCount�� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ResetNullCount()
    {
      _nullCount = 0;
    }

    /// <summary>
    /// ��Ʈ�� �˻� ���� �� _nullCount�� ������Ű��, ���� Ƚ�� �̻��̸� ���¸� �ʱ�ȭ�մϴ�.
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
          if (Settings.TargetAppName == AppKey.USarang)
          {
            if (!_searcher.FindWindowByTitle(title => title.Contains("�����") && title.Contains("������")))
              return false;
          }
          else if (Settings.TargetAppName == AppKey.Brain)
          {
            if (!_searcher.FindWindowByTitle(title => title.Contains("�����") && title.Contains("ver")))
              return false;
          }
          else
          {
            if (!_searcher.FindWindowByTitle(title => title.Contains("�����[")))
              return false;
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
        chartTextBox = controls.FirstOrDefault(c => c.ControlType == "ControlType.Edit" && c.Index == 1);
        nameTextBox = controls.FirstOrDefault(c => c.ControlType == "ControlType.Edit" && c.Index == 7);
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

    private AutomationAppControls? FindUSarangControls(List<AutomationControlInfo> controls)
    {
      // ControlType.Edit�� ��Ʈ�ѵ��� ���͸�
      var editControls = controls.Where(c => c.ControlType == "ControlType.Edit").ToList();

      if (editControls.Count < 2) return null;

      // �� ���ο� ���� ���� ��Ʈ���� �ִ� �׷� ã��
      var maxGroup = editControls
          .GroupBy(x => x.RectTop)
          .OrderByDescending(g => g.Count())
          .FirstOrDefault();

      var chartControl = maxGroup?.First();

      // ������ ��Ʈ�� �߿��� X���� ���� ����� ��Ʈ���� ã��
      var nameControl = maxGroup?.Where(c => c != chartControl).OrderBy(c => Math.Abs(c.BoundingRectangle.Left - chartControl?.BoundingRectangle.Left ?? 0)).First();
      chartControl = _searcher.CreateControlInfo(chartControl?.Element);

      if (chartControl == null || nameControl == null)
        return null;

      //// �׽�Ʈ
      //chartControl = editControls[2];
      //nameControl = editControls[3];



      // OCR ����
      try
      {
        IntPtr hWnd = _searcher.GetWindowHandle();
        if (hWnd != IntPtr.Zero)
        {
          // �������� ���� ��ǥ ���
          Vanara.PInvoke.User32.GetWindowRect(new Vanara.PInvoke.HWND(hWnd), out var windowRect);

          // ���� ��ǥ�� ������ ��� ��ǥ�� ��ȯ
          var chartRectRelative = new Rectangle(chartControl.BoundingRectangle.X - windowRect.X, chartControl.BoundingRectangle.Y - windowRect.Y, chartControl.BoundingRectangle.Width, chartControl.BoundingRectangle.Height);
          var chartImg = _windowCaptureService.CaptureWindow(hWnd, chartRectRelative);

          // ���� �̹����� �����ϸ� ĳ�õ� ���� ���
          if (OpenCvUtils.AreImagesSimilar(_previousImageResult.BitmapSource, chartImg, 1))
            return _appControls;

          _previousImageResult.BitmapSource = chartImg;

          // BitmapSource�� Bitmap���� ��ȯ �� OCR ����
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
        var filePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "USarangControls.txt");
        System.IO.File.WriteAllText(filePath, infoText);
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
      // ClickSoft �϶� Win32 API �켱 ���
      if (Settings.TargetAppName == AppKey.ClickSoft)
      {
        var win32Result = _clickSoftControlSearchService.FindControls();
        if (win32Result != null && win32Result.ChartTextBox != null && win32Result.NameTextBox != null)
        {
          _appControls.SetControls(win32Result.ChartTextBox, win32Result.NameTextBox);
          ResetNullCount();
          return CreatePatientInfo();
        }
        // Win32�� �� ã���� _nullCount ����
        IncrementAndCheckNullCount();
        // Win32�� �� ã���� Automation���� ��� �õ�
      }

      // ������ Ÿ��Ʋ�� �ڵ� ã��   
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

        if (hWnd == IntPtr.Zero) return patientInfo;

        BitmapSource? bitmapSource = _windowCaptureService.CaptureWindow(hWnd, captureRect: _settingsService.Settings.ParseCustomImageRect());
        if (bitmapSource == null)
        {
          _previousImageResult.Clear();
        }
        // ���� �̹����� �����ϸ� ĳ�õ� ���� ���
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
        if (_settingsService.Settings.TargetAppName != AppKey.USarang) // �ǻ���� OCR�� ó���ϹǷ� ĳ�� ����
        {
          // ���� ������� ĳ�õ� ��Ʈ�� ���

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

        var controls = _searcher.FoundControls.Count != 0
          ? _searcher.FoundControls
       : _searcher.SearchControls();

        // ��Ʈ, �����ڸ� ��Ʈ�� ã�Ƽ� ����
        GetAppControls(controls);

        return CreatePatientInfo();
      }
    }


    private string ApplyRegexOrDefault(string? input, string pattern, int groupIndex)
    {
      if (!IsCustomNotImage || string.IsNullOrWhiteSpace(pattern))
        return input ?? string.Empty;

      Match match = Regex.Match(input ?? string.Empty, pattern);
      var group = match.Groups.Cast<Group>().ElementAtOrDefault(groupIndex);
      return group?.Value?.Trim() ?? string.Empty;
    }

    private PatientInfo CreatePatientInfo()
    {
      if (Settings.TargetAppName == AppKey.Brain)
      {
        var splitResults = _appControls.ChartTextBox?.Text?.Split(" ", 2);
        if (splitResults != null && splitResults.Length >= 2)
          return new PatientInfo
          {
            Chart = splitResults[0].Trim(),
            Name = splitResults[1].Trim(),
          };
      }

      string chart = ApplyRegexOrDefault(_appControls.ChartTextBox?.Text, Settings.CustomChartRegex, Settings.CustomChartRegexIndex);
      string name = ApplyRegexOrDefault(_appControls.NameTextBox?.Text, Settings.CustomNameRegex, Settings.CustomNameRegexIndex);

      return new PatientInfo
      {
        Chart = chart,
        Name = name,
      };
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
    public PatientInfo? PatientInfo { get; set; }
    public BitmapSource? BitmapSource { get; set; }

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
