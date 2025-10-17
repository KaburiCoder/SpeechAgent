using CommunityToolkit.Mvvm.Messaging.Messages;
using SpeechAgent.Models;

namespace SpeechAgent.Messages;

public class PatientInfoUpdatedMessage : ValueChangedMessage<PatientInfo>
{
  public PatientInfoUpdatedMessage(PatientInfo patientInfo) : base(patientInfo) { }
}