namespace SpeechAgent.Models
{
  public record PatientInfo(string Chart, string Name, DateTime Timestamp)
  {
    public PatientInfo() : this(string.Empty, string.Empty, DateTime.Now) { }
    public PatientInfo(string Chart, string Name) : this(Chart, Name, DateTime.Now) { }

    public bool IsEqual(PatientInfo other)
    {
      return this.Chart.Trim() == other.Chart.Trim() && this.Name.Trim() == other.Name.Trim();
    }
     
    public bool HasOnlyOneInfo()
    {
      bool hasChart = !string.IsNullOrWhiteSpace(Chart);
      bool hasName = !string.IsNullOrWhiteSpace(Name);
      return hasChart ^ hasName;
    }
  }
}
