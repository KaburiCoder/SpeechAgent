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
        Icon = SystemIcons.Application, // �⺻ ������ ���, �ʿ�� Ŀ���� ���������� ����
        Text = "Voice Medic Agent",
        Visible = false
      };

      // ���ؽ�Ʈ �޴� ����
      var contextMenu = new ContextMenuStrip();

      var showMenuItem = new ToolStripMenuItem("���̱�", null, OnShow);
      var exitMenuItem = new ToolStripMenuItem("����", null, OnExit);

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

      // Ʈ���̷� �ּ�ȭ�Ǿ����� �˸��� ǳ�� ��
      _notifyIcon.ShowBalloonTip(2000, "Voice Medic Agent", "Ʈ���̷� �ּ�ȭ�Ǿ����ϴ�.", ToolTipIcon.Info);
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