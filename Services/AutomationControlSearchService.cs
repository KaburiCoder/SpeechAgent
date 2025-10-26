using SpeechAgent.Constants;
using SpeechAgent.Database.Schemas;
using SpeechAgent.Features.Settings;
using SpeechAgent.Models;
using SpeechAgent.Utils.Automation;

namespace SpeechAgent.Services
{
  public interface IAutomationControlSearchService
  {
    void Clear();
    AutomationAppControls? FindChartAndNameControls();
  }

  public class AutomationControlSearchService : IAutomationControlSearchService
  {
    private AutomationControlSearcher _searcher = new();
    private AutomationAppControls _appControls = new();
    private readonly ISettingsService _settingsService;

    public AutomationControlSearchService(ISettingsService settingsService)
    {
      _settingsService = settingsService;
    }

    public AutomationAppControls? FindChartAndNameControls()
    {
      // 설정 로드
      var settings = _settingsService.Settings;

      bool isNewCreated = false;
      if (!_searcher.IsWindowValid())
      {
        _searcher.ClearFoundControls();
        _appControls.ClearControls();

        bool isCustom = settings.TargetAppName == AppKey.CustomUser || settings.TargetAppName == AppKey.CustomUserImage;

        if (isCustom)
        {
          if (!_searcher.FindWindowByTitle(title => title.Contains(settings.CustomExeTitle)))
            return null;
        }
        else
        {
          if (!_searcher.FindWindowByTitle(title => title.Contains("진료실[")))
            return null;
        }
        isNewCreated = true;
      }

      if (!isNewCreated && _appControls.ChartTextBox != null && _appControls.NameTextBox != null)
      {
        _appControls.ChartTextBox.Text = _searcher.GetControlText(_appControls.ChartTextBox.Element);
        _appControls.NameTextBox.Text = _searcher.GetControlText(_appControls.NameTextBox.Element);
        return _appControls;
      }

      var controls = _searcher.FoundControls.Count != 0
        ? _searcher.FoundControls
        : _searcher.SearchControls();

      AutomationAppControls? result = null;
      if (settings.TargetAppName == "[사용자 정의]")
      {
        result = FindCustomControls(controls, settings);
      }
      else if (settings.TargetAppName == "[사용자 정의(이미지)]")
      {
        result = FindImageBasedControls(controls, settings);
      }
      else if (settings.TargetAppName == "클릭")
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

    private AutomationAppControls? FindImageBasedControls(List<AutomationControlInfo> controls, LocalSettings settings)
    {
      if (string.IsNullOrEmpty(settings.CustomImageRect))
        return null;

      // CustomImageRect 파싱 (x,y,width,height)
      var parts = settings.CustomImageRect.Split(',');
      if (parts.Length != 4)
        return null;

      if (!int.TryParse(parts[0], out int targetX) ||
              !int.TryParse(parts[1], out int targetY) ||
              !int.TryParse(parts[2], out int targetWidth) ||
        !int.TryParse(parts[3], out int targetHeight))
        return null;

      var targetRect = new System.Drawing.Rectangle(targetX, targetY, targetWidth, targetHeight);

      // 지정된 영역과 겹치는 컨트롤 찾기
      var matchingControls = controls.Where(c =>
       {
         var controlRect = c.BoundingRectangle;
         return targetRect.IntersectsWith(controlRect);
       }).ToList();

      // 가장 겹치는 영역이 큰 컨트롤 선택
      var bestMatch = matchingControls.OrderByDescending(c =>
      {
        var intersection = System.Drawing.Rectangle.Intersect(targetRect, c.BoundingRectangle);
        return intersection.Width * intersection.Height;
      }).FirstOrDefault();

      if (bestMatch != null)
      {
        var appControls = new AutomationAppControls();
        appControls.SetControls(bestMatch, null);
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
}
