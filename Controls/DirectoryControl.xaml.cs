using System.Windows;

namespace SpeechAgent.Controls
{
  /// <summary>
  /// DirectoryControl.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class DirectoryControl : System.Windows.Controls.UserControl
  {
    public DirectoryControl()
    {
      InitializeComponent();
    }

    public string DirPath
    {
      get { return (string)GetValue(DirPathProperty); }
      set { SetValue(DirPathProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DirPath.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DirPathProperty = DependencyProperty.Register(
      "DirPath",
      typeof(string),
      typeof(DirectoryControl),
      new FrameworkPropertyMetadata(
        null,
        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        new PropertyChangedCallback(OnDirPathChanged)
      )
    );

    private static void OnDirPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var control = (DirectoryControl)d;
      if (control.DirectoryPathTextBox != null)
      {
        control.DirectoryPathTextBox.Text = (string)e.NewValue ?? string.Empty;
      }
    }

    private void BrowseDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
      using (var dialog = new FolderBrowserDialog())
      {
        dialog.Description = "디렉토리를 선택하세요";
        dialog.ShowNewFolderButton = true;

        // 현재 텍스트 박스의 경로를 초기 경로로 설정
        if (
          !string.IsNullOrWhiteSpace(DirectoryPathTextBox.Text)
          && System.IO.Directory.Exists(DirectoryPathTextBox.Text)
        )
        {
          dialog.SelectedPath = DirectoryPathTextBox.Text;
        }

        DialogResult result = dialog.ShowDialog();
        if (result == DialogResult.OK)
        {
          DirectoryPathTextBox.Text = dialog.SelectedPath;
        }
      }
    }
  }
}
