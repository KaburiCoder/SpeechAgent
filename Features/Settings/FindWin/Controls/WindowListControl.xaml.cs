using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using UserControl = System.Windows.Controls.UserControl;
using SpeechAgent.Features.Settings.FindWin.Models;

namespace SpeechAgent.Features.Settings.FindWin.Controls
{
  /// <summary>
  /// WindowListControl.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class WindowListControl : UserControl
  {
    public static readonly DependencyProperty WindowsProperty =
      DependencyProperty.Register(nameof(Windows), typeof(ObservableCollection<WindowInfo>), typeof(WindowListControl), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedWindowProperty =
      DependencyProperty.Register(nameof(SelectedWindow), typeof(WindowInfo), typeof(WindowListControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ShowLoadButtonProperty =
      DependencyProperty.Register(nameof(ShowLoadButton), typeof(bool), typeof(WindowListControl), new PropertyMetadata(true));

    public ObservableCollection<WindowInfo> Windows
    {
      get => (ObservableCollection<WindowInfo>)GetValue(WindowsProperty);
      set => SetValue(WindowsProperty, value);
    }

    public WindowInfo SelectedWindow
    {
      get => (WindowInfo)GetValue(SelectedWindowProperty);
      set => SetValue(SelectedWindowProperty, value);
    }

    public bool ShowLoadButton
    {
      get => (bool)GetValue(ShowLoadButtonProperty);
      set => SetValue(ShowLoadButtonProperty, value);
    }

    public WindowListControl()
    {
      InitializeComponent();
    }
  }
}
