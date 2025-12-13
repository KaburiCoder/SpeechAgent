using System.Windows.Media;
using System.Windows.Threading;
using Notification.Wpf;
using SpeechAgent.Services.Api;
using SpeechAgent.Utils;

namespace SpeechAgent.Services
{
  public interface IUserNotificationService
  {
    void StartIntervalFeedbackNotification();

    void StopPeriodicNotification();
  }

  internal class UserNotificationService : IUserNotificationService
  {
    private readonly NotificationManager _notificationManager;
    private readonly IUserNotificationsApi _userNotificationsApi;
    private DispatcherTimer? _notificationTimer;

    public UserNotificationService(IUserNotificationsApi userNotificationsApi)
    {
      _notificationManager = new NotificationManager();
      this._userNotificationsApi = userNotificationsApi;
    }

    public void StartIntervalFeedbackNotification()
    {
      // 기존 타이머 중지
      StopPeriodicNotification();

      var displayInterval = TimeSpan.FromSeconds(5);

      // 주기적 알림을 위한 타이머 시작
      _notificationTimer = new DispatcherTimer { Interval = displayInterval };
      _notificationTimer.Tick += _notificationTimer_Tick;
      _notificationTimer.Start();
    }

    private async void _notificationTimer_Tick(object? sender, EventArgs e)
    {
      var results = await _userNotificationsApi.MarkAllAsAlert(
        new Api.Dto.UserNotificationMarkAlertDto("feedback")
      );

      // targetId 기준으로 중복 제거
      results = results.GroupBy(item => item.TargetId).Select(g => g.First());

      foreach (var item in results)
      {
        ShowNotification(
          "[Voice Medic] 알림이 도착했어요!",
          item.Title,
          NotificationType.Notification,
          () =>
          {
            BrowserLauncher.OpenUrl($"https://medic.clickcns.com/feedback/{item.TargetId}");
          },
          () => { }
        );
      }
    }

    public void StopPeriodicNotification()
    {
      if (_notificationTimer != null)
      {
        _notificationTimer.Stop();
        _notificationTimer = null;
      }
    }

    private void ShowNotification(
      string title,
      string message,
      NotificationType type,
      Action? onLeftButtonClick,
      Action? onRightButtonClick
    )
    {
      var content = new NotificationContent
      {
        Title = title,
        Message = message,
        Type = type,
        RowsCount = 3,
        LeftButtonContent = "확인",
        RightButtonContent = "취소",
        LeftButtonAction = onLeftButtonClick ?? (() => { }),
        RightButtonAction = onRightButtonClick ?? (() => { }),
        CloseOnClick = true,
        Background = new SolidColorBrush(Colors.DarkSlateBlue),
        Foreground = new SolidColorBrush(Colors.White),
      };

      _notificationManager.Show(content, expirationTime: TimeSpan.MaxValue);
    }
  }
}
