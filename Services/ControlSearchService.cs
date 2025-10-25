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

    public ControlSearchService() { }

    public AppControls? FindChartAndNameControls()
    {
      bool isNewCreated = false;
      if (!_searcher.IsHwndValid())
      {
        _searcher.ClearFoundControls();
        _appControls.ClearControls();
        if (!_searcher.FindWindowByTitle(title => title.StartsWith("진료실["))) return null;
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

      var chartLabel = controls
         .FirstOrDefault(c => c.Text.StartsWith("차트"));
      var nameLabel = controls
        .FirstOrDefault(c => c.Text.StartsWith("이름") || c.Text.StartsWith("수진자명"));
      var edits = controls.FindAll(c => c.ClassName.Contains("EDIT.app"));

      ControlInfo? chartEdit = null;
      ControlInfo? nameEdit = null;
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
        _appControls.SetControls(chartEdit, nameEdit);

        return _appControls;
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
