using SpeechAgent.Models;
using Vanara.PInvoke;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// Win32 API를 사용하여 윈도우 컨트롤을 검색하는 인터페이스
  /// </summary>
  public interface IControlSearcher
  {
    /// <summary>
    /// 현재 설정된 윈도우 핸들에서 자식 컨트롤들을 검색합니다.
 /// </summary>
    List<ControlInfo> SearchControls();

    /// <summary>
    /// 제목으로 윈도우를 찾고 핸들을 설정합니다.
    /// </summary>
    bool FindWindowByTitle(Func<string, bool> winTitlePredicate);

    /// <summary>
    /// 현재 윈도우 핸들이 유효한지 확인합니다.
    /// </summary>
    bool IsHwndValid();

 /// <summary>
    /// 찾은 컨트롤 목록을 초기화합니다.
    /// </summary>
    void ClearFoundControls();

    /// <summary>
    /// 윈도우 핸들을 설정합니다.
    /// </summary>
    void SetHwnd(HWND hwnd);

    /// <summary>
    /// 컨트롤의 텍스트를 가져옵니다.
    /// </summary>
    string GetControlText(HWND hwnd);

    /// <summary>
  /// 현재까지 찾은 컨트롤 목록을 반환합니다.
    /// </summary>
    List<ControlInfo> FoundControls { get; }
  }
}
