namespace SpeechAgent.Models
{
  public record PatientInfo(string Chart, string Name, DateTime Timestamp)
  {
    public PatientInfo(): this(string.Empty, string.Empty, DateTime.Now) { }
    public PatientInfo(string Chart, string Name) : this(Chart, Name, DateTime.Now) { }
  }
}
