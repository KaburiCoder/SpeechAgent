using SpeechAgent.Features.Main;
using SpeechAgent.Features.Settings;
using System.IO;
using System.Windows;

namespace SpeechAgent.Services
{
  public class TrayIconService : IDisposable
  {
    private NotifyIcon? _notifyIcon;
    private MainView? _mainView;
    private bool _disposed = false;
    private readonly ISettingsService _settingsService;

    public TrayIconService(ISettingsService settingsService)
    {
      _settingsService = settingsService;
    }

    public void Initialize(MainView mainView)
    {
      _mainView = mainView;
      CreateNotifyIcon();
      SetupMainViewEvents();

      // ConnectKey�� ������� ������ ���� �� �ٷ� Ʈ���̷� ����
      if (!string.IsNullOrWhiteSpace(_settingsService.ConnectKey))
      {
        HideToTray();
      }
    }

    private void CreateNotifyIcon()
    {
      _notifyIcon = new NotifyIcon
      {
        Icon = new Icon(new MemoryStream(Properties.Resources.main)), // Resources���� main.ico ���
        Text = "Voice Medic Agent",
      };

      // ���ؽ�Ʈ �޴� ����
      var contextMenu = new ContextMenuStrip();

      var font = new System.Drawing.Font("���� ���", 10F);
      var showMenuItem = new ToolStripMenuItem("���̱�", null, OnShow) { Font = font };
      var exitMenuItem = new ToolStripMenuItem("����", null, OnExit) { Font = font };

      contextMenu.Items.Add(showMenuItem);
      contextMenu.Items.Add(new ToolStripSeparator());
      contextMenu.Items.Add(exitMenuItem);

      _notifyIcon.ContextMenuStrip = contextMenu;

      // ����Ŭ�� �� â ���̱�
      _notifyIcon.DoubleClick += OnShow;
    }

    private void SetupMainViewEvents()
    {
      if (_mainView == null) return;

      // â �ݱ� �̺�Ʈ ó��
      _mainView.Closing += OnMainViewClosing;

      // â ���� ���� �̺�Ʈ ó�� (�ּ�ȭ �� Ʈ���̷�)
      _mainView.StateChanged += OnMainViewStateChanged;
    }

    private void OnMainViewClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
      // â �ݱ⸦ ����ϰ� Ʈ���̷� �ּ�ȭ
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

      _notifyIcon.ShowBalloonTip(1500, "Voice Medic Agent", "Minimized to tray.", ToolTipIcon.Info);
    }

    private void OnShow(object? sender, EventArgs e)
    {
      if (_mainView == null || _notifyIcon == null) return;

      _mainView.Show();
      _mainView.WindowState = WindowState.Normal;
      _mainView.Activate();
    }

    private void OnExit(object? sender, EventArgs e)
    {
      // ������ ���ø����̼� ����
      if (_mainView != null)
      {
        _mainView.Closing -= OnMainViewClosing; // �̺�Ʈ ����
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