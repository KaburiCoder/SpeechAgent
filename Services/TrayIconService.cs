using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using SpeechAgent.Features.Main;

namespace SpeechAgent.Services
{
  public class TrayIconService : IDisposable
  {
    private NotifyIcon? _notifyIcon;
    private MainView? _mainView;
    private bool _disposed = false;

    public void Initialize(MainView mainView)
    {
      _mainView = mainView;
      CreateNotifyIcon();
      SetupMainViewEvents();
    }

    private void CreateNotifyIcon()
    {
      _notifyIcon = new NotifyIcon
      {
        Icon = SystemIcons.Application, // 기본 아이콘 사용, 필요시 커스텀 아이콘으로 변경
        Text = "Voice Medic Agent",
        Visible = false
      };

      // 컨텍스트 메뉴 생성
      var contextMenu = new ContextMenuStrip();

      var showMenuItem = new ToolStripMenuItem("보이기", null, OnShow);
      var exitMenuItem = new ToolStripMenuItem("종료", null, OnExit);

      contextMenu.Items.Add(showMenuItem);
      contextMenu.Items.Add(new ToolStripSeparator());
      contextMenu.Items.Add(exitMenuItem);

      _notifyIcon.ContextMenuStrip = contextMenu;

      // 더블클릭 시 창 보이기
      _notifyIcon.DoubleClick += OnShow;
    }

    private void SetupMainViewEvents()
    {
      if (_mainView == null) return;

      // 창 닫기 이벤트 처리
      _mainView.Closing += OnMainViewClosing;

      // 창 상태 변경 이벤트 처리 (최소화 시 트레이로)
      _mainView.StateChanged += OnMainViewStateChanged;
    }

    private void OnMainViewClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
      // 창 닫기를 취소하고 트레이로 최소화
      e.Cancel = true;
      HideToTray();
    }

    private void OnMainViewStateChanged(object? sender, EventArgs e)
    {
      if (_mainView?.WindowState == WindowState.Minimized)
      {
        HideToTray();
      }
    }

    private void HideToTray()
    {
      if (_mainView == null || _notifyIcon == null) return;

      _mainView.Hide();
      _notifyIcon.Visible = true;

      // 트레이로 최소화되었음을 알리는 풍선 팁
      _notifyIcon.ShowBalloonTip(2000, "Voice Medic Agent", "트레이로 최소화되었습니다.", ToolTipIcon.Info);
    }

    private void OnShow(object? sender, EventArgs e)
    {
      if (_mainView == null || _notifyIcon == null) return;

      _mainView.Show();
      _mainView.WindowState = WindowState.Normal;
      _mainView.Activate();
      _notifyIcon.Visible = false;
    }

    private void OnExit(object? sender, EventArgs e)
    {
      // 실제로 애플리케이션 종료
      if (_mainView != null)
      {
        _mainView.Closing -= OnMainViewClosing; // 이벤트 해제
        _mainView.Close();
      }

      System.Windows.Application.Current.Shutdown();
    }

    public void ShowFromTray()
    {
      OnShow(null, EventArgs.Empty);
    }

    public void Dispose()
    {
      if (_disposed) return;

      if (_mainView != null)
      {
        _mainView.Closing -= OnMainViewClosing;
        _mainView.StateChanged -= OnMainViewStateChanged;
      }

      if (_notifyIcon != null)
      {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
      }

      _disposed = true;
    }
  }
}