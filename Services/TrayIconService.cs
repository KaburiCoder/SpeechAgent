using System.IO;
using System.Windows;
using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using SpeechAgent.Utils;

namespace SpeechAgent.Services
{
  public class TrayIconService : IDisposable
  {
    private NotifyIcon? _notifyIcon;
    private MainView? _mainView;
    private bool _disposed = false;
    private readonly ISettingsService _settingsService;
    private readonly IViewService _viewService;

    public TrayIconService(ISettingsService settingsService, IViewService viewService)
    {
      _settingsService = settingsService;
      this._viewService = viewService;
    }

    public void Initialize(MainView mainView)
    {
      _mainView = mainView;
      CreateNotifyIcon();
      SetupMainViewEvents();

      // ConnectKey가 비어있지 않으면 실행 시 바로 트레이로 숨김
      if (!string.IsNullOrWhiteSpace(_settingsService.Settings.ConnectKey))
      {
        HideToTray();
      }
    }

    private void CreateNotifyIcon()
    {
      _notifyIcon = new NotifyIcon
      {
        Icon = new Icon(new MemoryStream(Properties.Resources.main)), // Resources에서 main.ico 사용
        Text = "Voice Medic Agent",
        Visible = true,
      };

      // 컨텍스트 메뉴 생성
      var contextMenu = new ContextMenuStrip();
      var showMenuItem = new ToolStripMenuItem("보이기", null, OnShow);
      var updateHistoryMenuItem = new ToolStripMenuItem("업데이트 확인", null, OnShowUpdateHistory);
      var popupVoiceMedicMenuItem = new ToolStripMenuItem(
        "VoiceMedic 브라우저 열기",
        null,
        OnPopupVoiceMedicBrowser
      );
      var versionLabel = new ToolStripLabel($"버전: {GetApplicationVersion()}");
      var exitMenuItem = new ToolStripMenuItem("종료", null, OnExit);

      contextMenu.Items.Add(showMenuItem);
      contextMenu.Items.Add(popupVoiceMedicMenuItem);
      contextMenu.Items.Add(new ToolStripSeparator());
      contextMenu.Items.Add(versionLabel);
      contextMenu.Items.Add(updateHistoryMenuItem);
      contextMenu.Items.Add(new ToolStripSeparator());
      contextMenu.Items.Add(exitMenuItem);

      _notifyIcon.ContextMenuStrip = contextMenu;

      // 더블클릭 시 창 보이기
      _notifyIcon.DoubleClick += OnShow;
    }

    private string GetApplicationVersion()
    {
      var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      return version?.ToString() ?? "Unknown";
    }

    private void OnPopupVoiceMedicBrowser(object? sender, EventArgs e)
    {
      BrowserLauncher.OpenMedic();
    }

    private void SetupMainViewEvents()
    {
      if (_mainView == null)
        return;

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
      if (_mainView == null || _notifyIcon == null)
        return;

      _mainView.Hide();
      _notifyIcon.Visible = true;

      _notifyIcon.ShowBalloonTip(1500, "Voice Medic Agent", "Minimized to tray.", ToolTipIcon.Info);
    }

    private void OnShow(object? sender, EventArgs e)
    {
      if (_mainView == null || _notifyIcon == null)
        return;

      _mainView.Show();
      _mainView.WindowState = WindowState.Normal;
      _mainView.Activate();
    }

    private void OnShowUpdateHistory(object? sender, EventArgs e)
    {
      if (_mainView == null)
        return;
      _viewService.ShowUpdateHistoryView(_mainView);
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
      if (_disposed)
        return;

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
