using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpeechAgent.Controls
{
  /// <summary>
  /// CustomTitleBar.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class CustomTitleBar : System.Windows.Controls.UserControl
  {
    public CustomTitleBar()
    {
      InitializeComponent();
    }

    #region Dependency Properties

  /// <summary>
  /// 타이틀 텍스트
    /// </summary>
    public string Title
  {
    get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(
      nameof(Title),
    typeof(string),
        typeof(CustomTitleBar),
        new PropertyMetadata("Title"));

  /// <summary>
    /// 아이콘 종류 (MaterialDesign PackIcon Kind)
    /// </summary>
    public string IconKind
    {
      get { return (string)GetValue(IconKindProperty); }
  set { SetValue(IconKindProperty, value); }
    }

 public static readonly DependencyProperty IconKindProperty =
      DependencyProperty.Register(
        nameof(IconKind),
        typeof(string),
        typeof(CustomTitleBar),
    new PropertyMetadata("CogOutline"));

    /// <summary>
    /// 닫기 버튼 클릭 커맨드
    /// </summary>
    public ICommand CloseCommand
 {
      get { return (ICommand)GetValue(CloseCommandProperty); }
      set { SetValue(CloseCommandProperty, value); }
    }

    public static readonly DependencyProperty CloseCommandProperty =
      DependencyProperty.Register(
        nameof(CloseCommand),
        typeof(ICommand),
        typeof(CustomTitleBar),
        new PropertyMetadata(null));

    #endregion

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var window = Window.GetWindow(this);
      if (window != null && e.ClickCount == 1)
      {
        window.DragMove();
      }
    }
  }
}
