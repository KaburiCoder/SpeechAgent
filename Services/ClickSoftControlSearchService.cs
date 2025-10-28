using SpeechAgent.Models;
using SpeechAgent.Utils;
using System.Drawing;

namespace SpeechAgent.Services
{
  /// <summary>
  /// ClickSoft 앱에서 Win32 API를 사용하여 컨트롤을 검색하는 서비스
  /// </summary>
  public interface IClickSoftControlSearchService
  {
    AutomationAppControls? FindControls();
    void Clear();
  }

  public class ClickSoftControlSearchService : IClickSoftControlSearchService
  {
    private readonly IControlSearcher _controlSearcher;

    public ClickSoftControlSearchService(IControlSearcher controlSearcher)
    {
      _controlSearcher = controlSearcher;
    }

    /// <summary>
    /// ClickSoft 앱에서 Win32 API를 사용하여 컨트롤을 찾습니다.
    /// </summary>
    public AutomationAppControls? FindControls()
    {
      try
      {
        // 윈도우 찾기
        if (!_controlSearcher.IsHwndValid())
        {
          if (!_controlSearcher.FindWindowByTitle(title => title.Contains("진료실[")))
          {
            Clear();
            return null;
          }
        }

        // 컨트롤 검색
        var controls = _controlSearcher.FoundControls.Count != 0
          ? _controlSearcher.FoundControls
          : _controlSearcher.SearchControls();

        if (controls.Count == 0)
          return null;

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

        return FindControlsFromList(controls);
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// 컨트롤 목록에서 차트 및 이름 컨트롤을 찾습니다.
    /// </summary>
    private AutomationAppControls? FindControlsFromList(List<ControlInfo> controls)
    {
      ControlInfo? chartEdit = null;
      ControlInfo? nameEdit = null;

      var chartLabel = controls.FirstOrDefault(c => c.Text.StartsWith("차트"));
      var nameLabel = controls.FirstOrDefault(c => c.Text.StartsWith("이름") || c.Text.StartsWith("수진자명"));
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
        return ConvertToAutomationAppControls(chartEdit, nameEdit);
      }

      return null;
    }

    /// <summary>
    /// ControlInfo를 AutomationControlInfo로 변환하여 AutomationAppControls를 생성합니다.
    /// </summary>
    private AutomationAppControls ConvertToAutomationAppControls(ControlInfo chartEdit, ControlInfo? nameEdit)
    {
      var appControls = new AutomationAppControls();

      var chartControlInfo = new AutomationControlInfo
      {
        ClassName = chartEdit.ClassName,
        Text = _controlSearcher.GetControlText(chartEdit.Hwnd),
        BoundingRectangle = new Rectangle(
          chartEdit.RECT.Left,
          chartEdit.RECT.Top,
          chartEdit.RECT.Right - chartEdit.RECT.Left,
          chartEdit.RECT.Bottom - chartEdit.RECT.Top)
      };

      AutomationControlInfo? nameControlInfo = null;
      if (nameEdit != null)
      {
        nameControlInfo = new AutomationControlInfo
        {
          ClassName = nameEdit.ClassName,
          Text = _controlSearcher.GetControlText(nameEdit.Hwnd),
          BoundingRectangle = new Rectangle(
            nameEdit.RECT.Left,
            nameEdit.RECT.Top,
            nameEdit.RECT.Right - nameEdit.RECT.Left,
            nameEdit.RECT.Bottom - nameEdit.RECT.Top)
        };
      }

      appControls.SetControls(chartControlInfo, nameControlInfo);
      return appControls;
    }

    /// <summary>
    /// 검색 상태를 초기화합니다.
    /// </summary>
    public void Clear()
    {
      _controlSearcher.ClearFoundControls();
    }
  }
}
