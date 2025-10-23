namespace SpeechAgent.Database.Schemas
{
  public class LocalSettings
  {
    public int Id { get; set; }
    public string ConnectKey { get; set; } = "";
    public string TargetAppName { get; set; } = "";
  }
}
