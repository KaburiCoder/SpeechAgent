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
  /// ControlPosBox.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class ControlPosBox : System.Windows.Controls.UserControl
  {
    public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(nameof(Title), typeof(string), typeof(ControlPosBox), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ControlTypeProperty =
      DependencyProperty.Register(nameof(ControlType), typeof(string), typeof(ControlPosBox), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IndexProperty =
      DependencyProperty.Register(nameof(Index), typeof(string), typeof(ControlPosBox), new PropertyMetadata(string.Empty));

    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public string ControlType
    {
      get { return (string)GetValue(ControlTypeProperty); }
      set { SetValue(ControlTypeProperty, value); }
    }

    public string Index
    {
      get { return (string)GetValue(IndexProperty); }
      set { SetValue(IndexProperty, value); }
    }

    public ControlPosBox()
    {
      InitializeComponent();
    }
  }
}
