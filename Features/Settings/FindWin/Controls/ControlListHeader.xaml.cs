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

namespace SpeechAgent.Features.Settings.FindWin.Controls
{
  /// <summary>
  /// ControlListHeader.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class ControlListHeader : System.Windows.Controls.UserControl
  {
    public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(nameof(Title), typeof(string), typeof(ControlListHeader), new PropertyMetadata("선택된 윈도우의 컨트롤 목록"));

    public static readonly DependencyProperty WindowTitleProperty =
      DependencyProperty.Register(nameof(WindowTitle), typeof(string), typeof(ControlListHeader), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ControlCountProperty =
      DependencyProperty.Register(nameof(ControlCount), typeof(int), typeof(ControlListHeader), new PropertyMetadata(0));

    public static readonly DependencyProperty IsLoadingProperty =
DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(ControlListHeader), new PropertyMetadata(false));

    public static readonly DependencyProperty LoadingTextProperty =
      DependencyProperty.Register(nameof(LoadingText), typeof(string), typeof(ControlListHeader), new PropertyMetadata("컨트롤 검색 중..."));

    public static readonly DependencyProperty SearchLabelProperty =
   DependencyProperty.Register(nameof(SearchLabel), typeof(string), typeof(ControlListHeader), new PropertyMetadata("텍스트 검색"));

    public static readonly DependencyProperty SearchTextProperty =
      DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(ControlListHeader), 
 new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SearchButtonTextProperty =
      DependencyProperty.Register(nameof(SearchButtonText), typeof(string), typeof(ControlListHeader), new PropertyMetadata("검색"));

    public static readonly DependencyProperty SearchCommandProperty =
      DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(ControlListHeader), new PropertyMetadata(null));


    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public string WindowTitle
    {
      get { return (string)GetValue(WindowTitleProperty); }
      set { SetValue(WindowTitleProperty, value); }
    }

    public int ControlCount
    {
      get { return (int)GetValue(ControlCountProperty); }
      set { SetValue(ControlCountProperty, value); }
    }

    public bool IsLoading
    {
      get { return (bool)GetValue(IsLoadingProperty); }
      set { SetValue(IsLoadingProperty, value); }
    }

    public string LoadingText
    {
      get { return (string)GetValue(LoadingTextProperty); }
      set { SetValue(LoadingTextProperty, value); }
    }

    public string SearchLabel
    {
      get { return (string)GetValue(SearchLabelProperty); }
      set { SetValue(SearchLabelProperty, value); }
    }

    public string SearchText
    {
      get { return (string)GetValue(SearchTextProperty); }
      set { SetValue(SearchTextProperty, value); }
    }

    public string SearchButtonText
    {
      get { return (string)GetValue(SearchButtonTextProperty); }
      set { SetValue(SearchButtonTextProperty, value); }
    }

    public ICommand SearchCommand
    {
      get { return (ICommand)GetValue(SearchCommandProperty); }
      set { SetValue(SearchCommandProperty, value); }
    }

    public ControlListHeader()
    {
      InitializeComponent();
    }
  }
}
