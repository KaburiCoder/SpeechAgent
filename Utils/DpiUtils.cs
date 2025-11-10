using System.Drawing;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// DPI(화면 배율) 관련 유틸리티 클래스
  /// </summary>
  public static class DpiUtils
  {
    /// <summary>
    /// 주어진 윈도우 핸들의 DPI 스케일 비율을 반환합니다. (1.0 = 100%, 1.25 = 125% 등)
    /// </summary>
    /// <param name="hWnd">윈도우 핸들</param>
    /// <returns>DPI 스케일 비율 (기본값: 1.0)</returns>
    public static double GetDpiScale(nint hWnd)
    {
      try
      {
        using (var graphics = Graphics.FromHwnd(hWnd))
        {
          double dpiX = graphics.DpiX / 96.0; // 96 = 100%
          LogUtils.WriteLog(
            LogLevel.Debug,
            $"[DpiUtils] DPI 스케일: {dpiX:F2} (DpiX: {graphics.DpiX})"
          );
          return dpiX;
        }
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Debug, $"[DpiUtils] DPI 스케일 조회 실패: {ex.Message}");
      }

      return 1.0; // 기본값: 100%
    }

    /// <summary>
    /// Automation 좌표를 윈도우 상대 좌표로 변환. (DPI 스케일 적용) 특성에 맞게 세부 조정 함
    /// </summary>
    public static Rectangle ConvertToWindowRelativeRect(
      Rectangle automationRect,
      Rectangle windowRect,
      double dpiScale
    )
    {
      var x = (automationRect.X - windowRect.X) / dpiScale;
      var y = (automationRect.Y - windowRect.Y) / dpiScale + (dpiScale == 1 ? 0 : 1);

      return new Rectangle((int)x, (int)y, automationRect.Width, automationRect.Height);
    }
  }
}
