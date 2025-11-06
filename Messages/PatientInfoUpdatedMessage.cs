using CommunityToolkit.Mvvm.Messaging.Messages;
using SpeechAgent.Models;

namespace SpeechAgent.Messages;

public class PatientInfoUpdatedMessage(PatientInfo patientInfo)
  : ValueChangedMessage<PatientInfo>(patientInfo) { }
