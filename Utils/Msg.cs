using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAgent.Utils
{
  public static class Msg
  {
    const string CAPTION = "SpeechAgent";

    public static void Show(string message)
    {
      System.Windows.MessageBox.Show(message, CAPTION);
    }
  }
}
