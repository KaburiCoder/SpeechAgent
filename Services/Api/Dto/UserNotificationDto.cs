using System;

namespace SpeechAgent.Services.Api.Dto
{
  public record UserNotificationDto(
    string Id,
    string UserId,
    string TargetId,
    string TargetType,
    string Title,
    bool IsRead,
    DateTime CreatedAt
  );

  public record UserNotificationMarkAlertDto(string TargetType);
}
