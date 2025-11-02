using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SpeechAgent.Database;
using SpeechAgent.Database.Schemas;

namespace SpeechAgent.Features.Settings
{
  public interface IShortcutSettingsService
  {
    /// <summary>
    /// 모든 저장된 단축키 로드
    /// </summary>
    List<CustomShortcuts> LoadAllShortcuts();

    /// <summary>
    /// 특정 기능의 단축키 조회
    /// </summary>
    CustomShortcuts? GetShortcut(ShortcutFeature feature);

    /// <summary>
    /// 단축키 저장 (없으면 추가, 있으면 업데이트)
    /// </summary>
    void SaveShortcut(ModifierKeys modifiers, Key key, ShortcutFeature feature);

    /// <summary>
    /// 단축키 삭제
    /// </summary>
    void DeleteShortcut(ModifierKeys modifiers, Key key);

    /// <summary>
    /// 모든 단축키 삭제
    /// </summary>
    void DeleteAllShortcuts();
  }

  public class ShortcutSettingsService : IShortcutSettingsService
  {
    public ShortcutSettingsService() { }

    /// <summary>
    /// 모든 저장된 단축키 로드
    /// </summary>
    public List<CustomShortcuts> LoadAllShortcuts()
    {
      try
      {
        using var dbContext = new AppDbContext();
        return dbContext.CustomShortcuts.ToList();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading shortcuts: {ex.Message}");
        return [];
      }
    }

    /// <summary>
    /// 특정 기능의 단축키 조회
    /// </summary>
    public CustomShortcuts? GetShortcut(ShortcutFeature feature)
    {
      try
      {
        using var dbContext = new AppDbContext();
        return dbContext.CustomShortcuts.FirstOrDefault(s => s.ShortcutFeature == feature);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error getting shortcut: {ex.Message}");
        return null;
      }
    }

    /// <summary>
    /// 단축키 저장 (없으면 추가, 있으면 업데이트)
    /// </summary>
    public void SaveShortcut(ModifierKeys modifiers, Key key, ShortcutFeature feature)
    {
      try
      {
        using var dbContext = new AppDbContext();
        // 기존 데이터 확인
        var existing = dbContext.CustomShortcuts.FirstOrDefault(s => s.ShortcutFeature == feature);

        if (existing != null)
        {
          // 업데이트
          existing.Modifiers = modifiers;
          existing.Key = key;
          existing.ShortcutFeature = feature;
          dbContext.CustomShortcuts.Update(existing);
        }
        else
        {
          // 새로 추가
          var shortcut = new CustomShortcuts
          {
            Modifiers = modifiers,
            Key = key,
            ShortcutFeature = feature,
          };
          dbContext.CustomShortcuts.Add(shortcut);
        }

        dbContext.SaveChanges();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error saving shortcut: {ex.Message}");
      }
    }

    /// <summary>
    /// 단축키 삭제
    /// </summary>
    public void DeleteShortcut(ModifierKeys modifiers, Key key)
    {
      try
      {
        using var dbContext = new AppDbContext();
        var shortcut = dbContext.CustomShortcuts.FirstOrDefault(s =>
          s.Modifiers == modifiers && s.Key == key
        );

        if (shortcut != null)
        {
          dbContext.CustomShortcuts.Remove(shortcut);
          dbContext.SaveChanges();
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error deleting shortcut: {ex.Message}");
      }
    }

    /// <summary>
    /// 모든 단축키 삭제
    /// </summary>
    public void DeleteAllShortcuts()
    {
      try
      {
        using var dbContext = new AppDbContext();
        dbContext.CustomShortcuts.RemoveRange(dbContext.CustomShortcuts);
        dbContext.SaveChanges();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error deleting all shortcuts: {ex.Message}");
      }
    }
  }
}
