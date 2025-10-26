using SpeechAgent.Database.Schemas;
using SpeechAgent.Features.Settings;
using SpeechAgent.Models;
using SpeechAgent.Utils;

namespace SpeechAgent.Services
{
  public interface IControlSearchService
  {
    AppControls? FindChartAndNameControls();
  }

  public class ControlSearchService : IControlSearchService
  {
    private ControlSearcher _searcher = new();
    private AppControls _appControls = new();
    private readonly ISettingsService _settingsService;

    public ControlSearchService(ISettingsService settingsService)
    {
      _settingsService = settingsService;
    }

    public AppControls? FindChartAndNameControls()
    {
      // 설정 로드
      var settings = _settingsService.Settings;

      bool isNewCreated = false;
      if (!_searcher.IsHwndValid())
      {
        _searcher.ClearFoundControls();
        _appControls.ClearControls();

        if (settings.TargetAppName == "[사용자 정의]" && !string.IsNullOrEmpty(settings.CustomExeTitle))
        {
          if (!_searcher.FindWindowByTitle(title => title.Contains(settings.CustomExeTitle))) return null;
        }
        else
        {
          if (!_searcher.FindWindowByTitle(title => title.Contains("진료실["))) return null;
        }
        isNewCreated = true;
      }

      if (!isNewCreated && _appControls.ChartTextBox != null && _appControls.NameTextBox != null)
      {
        _appControls.ChartTextBox.Text = _searcher.GetControlText(_appControls.ChartTextBox.Hwnd);
        _appControls.NameTextBox.Text = _searcher.GetControlText(_appControls.NameTextBox.Hwnd);
        return _appControls;
      }

      var controls = _searcher.FoundControls.Count != 0
        ? _searcher.FoundControls
        : _searcher.SearchControls();

      // 클래스별로 그룹화하여 Index 재설정
      var grouped = controls.GroupBy(c => c.ClassName);
      foreach (var group in grouped)
      {
        int index = 0;
        foreach (var control in group)
        {
          control.Index = index++;
        }
      }

      AppControls? result = null;
      if (settings.TargetAppName == "[사용자 정의]")
      {
        result = FindCustomControls(controls, settings);
      }
      else
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

    private AppControls? FindCustomControls(List<ControlInfo> controls, LocalSettings settings)
    {
      ControlInfo? chartEdit = null;
      ControlInfo? nameEdit = null;

      if (!string.IsNullOrEmpty(settings.CustomChartControlType) && !string.IsNullOrEmpty(settings.CustomChartIndex))
      {
        var chartControls = controls.Where(c => c.ClassName == settings.CustomChartControlType).ToList();
        if (int.TryParse(settings.CustomChartIndex, out int chartIndex) && chartIndex < chartControls.Count)
        {
          chartEdit = chartControls[chartIndex];
        }
      }
      if (!string.IsNullOrEmpty(settings.CustomNameControlType) && !string.IsNullOrEmpty(settings.CustomNameIndex))
      {
        var nameControls = controls.Where(c => c.ClassName == settings.CustomNameControlType).ToList();
        if (int.TryParse(settings.CustomNameIndex, out int nameIndex) && nameIndex < nameControls.Count)
        {
          nameEdit = nameControls[nameIndex];
        }
      }

      if (chartEdit != null)
      {
        var appControls = new AppControls();
        appControls.SetControls(chartEdit, nameEdit);
        return appControls;
      }

      return null;
    }

    private AppControls? FindDefaultControls(List<ControlInfo> controls)
    {
      ControlInfo? chartEdit = null;
      ControlInfo? nameEdit = null;

      var chartLabel = controls
         .FirstOrDefault(c => c.Text.StartsWith("차트"));
      var nameLabel = controls
        .FirstOrDefault(c => c.Text.StartsWith("이름") || c.Text.StartsWith("수진자명"));
      var edits = controls.FindAll(c => c.ClassName.Contains("EDIT.app"));

      if (edits.Count == 0)
      {
        // NewClick
        chartEdit = controls.Where(x => x.ClassName.StartsWith("Edit")).ElementAtOrDefault(1);
        nameEdit = controls.Where(x => x.ClassName.StartsWith("ThunderRT6TextBox")).ElementAtOrDefault(0);
      }
      else if (chartLabel != null && nameLabel != null)
      {
        chartEdit = edits.FirstOrDefault(ed => ed.RECT.Left > chartLabel.RECT.Right &&
                                 Math.Abs(ed.RECT.Top - chartLabel.RECT.Top) < 5 &&
                                 Math.Abs(ed.RECT.Bottom - chartLabel.RECT.Bottom) < 5);
        nameEdit = edits.FirstOrDefault(ed => ed.RECT.Left > nameLabel.RECT.Right &&
                               Math.Abs(ed.RECT.Top - nameLabel.RECT.Top) < 5 &&
                               Math.Abs(ed.RECT.Bottom - nameLabel.RECT.Bottom) < 5);
      }

      if (chartEdit != null)
      {
        var appControls = new AppControls();
        appControls.SetControls(chartEdit, nameEdit);
        return appControls;
      }

      return null;
    }
  }

  public class AppControls
  {
    public ControlInfo? ChartTextBox { get; private set; }
    public ControlInfo? NameTextBox { get; private set; }

    public void SetControls(ControlInfo? chartTextBox, ControlInfo? nameTextBox)
    {
      ChartTextBox = chartTextBox;
      NameTextBox = nameTextBox;
    }

    internal void ClearControls()
    {
      ChartTextBox = null;
      NameTextBox = null;
    }
  }
}
