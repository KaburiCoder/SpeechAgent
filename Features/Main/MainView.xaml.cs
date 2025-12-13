using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Notification.Wpf;
using Notification.Wpf.Classes;
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

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      var notificationManager = new NotificationManager();
      var content = new NotificationContent
      {
        Title = "Sample notification",
        Message =
          "Lorem ipsum dolor sit amet, consectetur adipiscing elit.Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
        Type = NotificationType.Information,
        RowsCount = 3, //Will show 3 rows and trim after
        LeftButtonContent = "확인", // Content of left button
        RightButtonContent = "취소", // Content of right button
        LeftButtonAction = () => { },
        RightButtonAction = () => { },
        CloseOnClick = true, // Set true if u want close message when left mouse button click on message (base = true)
        Background = new SolidColorBrush(Colors.White),
        Foreground = new SolidColorBrush(Colors.DarkRed),
      };
      notificationManager.Show(content, expirationTime: TimeSpan.MaxValue);
    }
  }
}
