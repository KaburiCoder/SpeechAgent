using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpeechAgent.Utils
{
  internal static class RegexUtils
  {
    public static string GetRegexString(this string input, string pattern, int groupIndex)
    {
      if (string.IsNullOrWhiteSpace(pattern))
        return input ?? string.Empty;

      Match match = Regex.Match(input ?? string.Empty, pattern);
      var group = match.Groups.Cast<Group>().ElementAtOrDefault(groupIndex);
      return group?.Value?.Trim() ?? string.Empty;
    }
  }
}
