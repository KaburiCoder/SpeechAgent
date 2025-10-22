using System.Windows;
using System.Windows.Input;
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

    // 커스텀 타이틀바 이벤트 핸들러들
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ButtonState == MouseButtonState.Pressed)
      {
        this.DragMove();
      }
    }     

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
