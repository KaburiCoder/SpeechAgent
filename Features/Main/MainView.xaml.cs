using System.Windows;
using SpeechAgent.Services;

namespace SpeechAgent.Features.Main
{
  /// <summary>
  /// MainView.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class MainView : Window
  {
    private readonly TrayIconService _trayIconService;

    public MainView(TrayIconService trayIconService)
    {
      InitializeComponent();
      _trayIconService = trayIconService;
      // 창이 로드된 후 트레이 아이콘 초기화
      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _trayIconService.Initialize(this);
    }

    protected override void OnClosed(EventArgs e)
    {
      _trayIconService.Dispose();
      base.OnClosed(e);
    }
  }
}
