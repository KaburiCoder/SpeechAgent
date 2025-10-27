using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpeechAgent.Features.Settings.FindWin.Controls
{
  /// <summary>
  /// ScanButton.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class ScanButton : System.Windows.Controls.UserControl
  {
    public static readonly DependencyProperty ButtonTextProperty =
      DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(ScanButton), new PropertyMetadata("버튼"));

    public static readonly DependencyProperty CommandProperty =
      DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ScanButton), new PropertyMetadata(null));

    public static readonly DependencyProperty LoadingTextProperty =
      DependencyProperty.Register(nameof(LoadingText), typeof(string), typeof(ScanButton), new PropertyMetadata("로딩 중..."));

    public static readonly DependencyProperty IsLoadingProperty =
      DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(ScanButton), new PropertyMetadata(false));


    public string ButtonText
    {
      get { return (string)GetValue(ButtonTextProperty); }
      set { SetValue(ButtonTextProperty, value); }
    }

    public ICommand Command
    {
      get { return (ICommand)GetValue(CommandProperty); }
      set { SetValue(CommandProperty, value); }
    }

    public string LoadingText
    {
      get { return (string)GetValue(LoadingTextProperty); }
      set { SetValue(LoadingTextProperty, value); }
    }

    public bool IsLoading
    {
      get { return (bool)GetValue(IsLoadingProperty); }
      set { SetValue(IsLoadingProperty, value); }
    }

    public ScanButton()
    {
      InitializeComponent();
    }
  }
}
