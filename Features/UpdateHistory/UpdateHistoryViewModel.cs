using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpeechAgent.Bases;

namespace SpeechAgent.Features.UpdateHistory
{
  partial class UpdateHistoryViewModel : BaseViewModel
  {
    private readonly IUpdateHistoryService _updateHistoryService;

    [ObservableProperty]
    private ObservableCollection<UpdateFileInfo> updateFiles = new();

    [ObservableProperty]
    private UpdateFileInfo? selectedUpdate = null;

    [ObservableProperty]
    private string markdownContent = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    public UpdateHistoryViewModel(IUpdateHistoryService updateHistoryService)
    {
      _updateHistoryService = updateHistoryService;
    }

    partial void OnSelectedUpdateChanged(UpdateFileInfo? value)
    {
      _ = LoadSelectedUpdateContentAsync();
    }

    public override async void Initialize()
    {
      await LoadUpdateFilesAsync();
    }

    private async Task LoadUpdateFilesAsync()
    {
      IsLoading = true;
      try
      {
        var files = await _updateHistoryService.GetUpdateFilesAsync();
        UpdateFiles.Clear();
        foreach (var file in files)
        {
          UpdateFiles.Add(file);
        }

        // 첫 번째 파일 자동 선택
        if (UpdateFiles.Count > 0)
        {
          SelectedUpdate = UpdateFiles[0];
          await LoadSelectedUpdateContentAsync();
        }
      }
      finally
      {
        IsLoading = false;
      }
    }

    [RelayCommand]
    private async Task SelectUpdateAsync(UpdateFileInfo? updateFile)
    {
      if (updateFile == null)
        return;

      SelectedUpdate = updateFile;
      await LoadSelectedUpdateContentAsync();
    }

    private async Task LoadSelectedUpdateContentAsync()
    {
      if (SelectedUpdate == null)
      {
        MarkdownContent = string.Empty;
        return;
      }

      try
      {
        MarkdownContent = await _updateHistoryService.GetUpdateContentAsync(
          SelectedUpdate.FileName
        );
      }
      catch
      {
        MarkdownContent = "파일을 읽을 수 없습니다.";
      }
    }
  }
}
