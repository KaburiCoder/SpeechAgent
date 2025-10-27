using SpeechAgent.Models;
using Vanara.PInvoke;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// Win32 API�� ����Ͽ� ������ ��Ʈ���� �˻��ϴ� �������̽�
  /// </summary>
  public interface IControlSearcher
  {
    /// <summary>
    /// ���� ������ ������ �ڵ鿡�� �ڽ� ��Ʈ�ѵ��� �˻��մϴ�.
 /// </summary>
    List<ControlInfo> SearchControls();

    /// <summary>
    /// �������� �����츦 ã�� �ڵ��� �����մϴ�.
    /// </summary>
    bool FindWindowByTitle(Func<string, bool> winTitlePredicate);

    /// <summary>
    /// ���� ������ �ڵ��� ��ȿ���� Ȯ���մϴ�.
    /// </summary>
    bool IsHwndValid();

 /// <summary>
    /// ã�� ��Ʈ�� ����� �ʱ�ȭ�մϴ�.
    /// </summary>
    void ClearFoundControls();

    /// <summary>
    /// ������ �ڵ��� �����մϴ�.
    /// </summary>
    void SetHwnd(HWND hwnd);

    /// <summary>
    /// ��Ʈ���� �ؽ�Ʈ�� �����ɴϴ�.
    /// </summary>
    string GetControlText(HWND hwnd);

    /// <summary>
  /// ������� ã�� ��Ʈ�� ����� ��ȯ�մϴ�.
    /// </summary>
    List<ControlInfo> FoundControls { get; }
  }
}
