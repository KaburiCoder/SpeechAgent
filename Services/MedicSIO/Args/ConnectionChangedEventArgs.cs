namespace SpeechAgent.Services.MedicSIO.Args
{
  public class ConnectionChangedEventArgs : EventArgs
  {
    public bool IsConnected { get; set; }
  }
}
