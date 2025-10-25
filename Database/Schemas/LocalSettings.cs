namespace SpeechAgent.Database.Schemas
{
  public class LocalSettings
  {
    public int Id { get; set; }
    public string ConnectKey { get; set; } = "";
    public string TargetAppName { get; set; } = "";
    public string CustomExeTitle { get; set; } = "";
    public string CustomChartClass { get; set; } = "";
    public string CustomChartIndex { get; set; } = "";
    public string CustomNameClass { get; set; } = "";
    public string CustomNameIndex { get; set; } = "";

    public string CustomImageName { get; set; } = "";
    public string CustomImageRect { get; set; } = "";
  }
}
