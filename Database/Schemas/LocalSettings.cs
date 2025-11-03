namespace SpeechAgent.Database.Schemas
{
  public class LocalSettings
  {
    public int Id { get; set; }
    public string ConnectKey { get; set; } = "";
    public string TargetAppName { get; set; } = "";
    public string CustomExeTitle { get; set; } = "";
    public string CustomChartControlType { get; set; } = "";
    public string CustomChartIndex { get; set; } = "";
    public string CustomChartRegex { get; set; } = "";
    public int CustomChartRegexIndex { get; set; } = 0;
    public string CustomNameControlType { get; set; } = "";
    public string CustomNameIndex { get; set; } = "";
    public string CustomNameRegex { get; set; } = "";
    public int CustomNameRegexIndex { get; set; } = 0;
    public string CustomImageRect { get; set; } = "";

    public bool IsBootPopupBrowserEnabled { get; set; } = true;
  }
}
