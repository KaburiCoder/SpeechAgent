using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAgent.Features.UpdateHistory
{
  public class UpdateFileInfo
  {
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
  }

  public interface IUpdateHistoryService
  {
    Task<List<UpdateFileInfo>> GetUpdateFilesAsync();
    Task<string> GetUpdateContentAsync(string fileName);
  }

  public class UpdateHistoryService : IUpdateHistoryService
  {
    private readonly string _updatesPath;

    public UpdateHistoryService()
    {
      // 애플리케이션 디렉토리 기준으로 _Updates 폴더 경로 설정
      _updatesPath = Path.Combine(AppContext.BaseDirectory, "_Updates");
    }

    public async Task<List<UpdateFileInfo>> GetUpdateFilesAsync()
    {
      if (!Directory.Exists(_updatesPath))
        return new List<UpdateFileInfo>();

      var mdFiles = Directory
        .GetFiles(_updatesPath, "*.md")
        .OrderByDescending(f => Path.GetFileNameWithoutExtension(f), new VersionComparer())
        .Select(f => new UpdateFileInfo
        {
          FileName = Path.GetFileNameWithoutExtension(f),
          FilePath = f,
          DisplayName = $"v{Path.GetFileNameWithoutExtension(f)}",
        })
        .ToList();

      return await Task.FromResult(mdFiles);
    }

    public async Task<string> GetUpdateContentAsync(string fileName)
    {
      var filePath = Path.Combine(_updatesPath, $"{fileName}.md");

      if (!File.Exists(filePath))
        return string.Empty;

      return await File.ReadAllTextAsync(filePath, Encoding.GetEncoding("euc-kr"));
    }

    // 버전 번호 순으로 정렬하기 위한 비교기
    private class VersionComparer : IComparer<string>
    {
      public int Compare(string? x, string? y)
      {
        if (x == null || y == null)
          return 0;

        var xParts = x.Split('.').Select(p => int.TryParse(p, out var num) ? num : 0).ToArray();
        var yParts = y.Split('.').Select(p => int.TryParse(p, out var num) ? num : 0).ToArray();

        for (int i = 0; i < Math.Max(xParts.Length, yParts.Length); i++)
        {
          var xPart = i < xParts.Length ? xParts[i] : 0;
          var yPart = i < yParts.Length ? yParts[i] : 0;

          var result = xPart.CompareTo(yPart); // 내림차순
          if (result != 0)
            return result;
        }

        return 0;
      }
    }
  }
}
