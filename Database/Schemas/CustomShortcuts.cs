using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpeechAgent.Database.Schemas
{
  public class CustomShortcuts
  {
    public ModifierKeys Modifiers { get; set; }
    public Key Key { get; set; }
    public ShortcutFeature ShortcutFeature { get; set; }
  }

  public enum ShortcutFeature
  {
    All,
    CC,
    S,
    O,
    A,
    P,
  }
}
