using System.Net.Http;
using System.Net.Http.Json;
using SpeechAgent.Services.Api.Dto;

namespace SpeechAgent.Services.Api
{
  public interface IUserNotificationsApi
  {
    Task<IEnumerable<UserNotificationDto>> MarkAllAsAlert(UserNotificationMarkAlertDto dto);
  }

  internal class UserNotificationsApi : ApiBase, IUserNotificationsApi
  {
    public UserNotificationsApi(IHttpClientFactory httpClientFactory)
      : base(httpClientFactory) { }

    public async Task<IEnumerable<UserNotificationDto>> MarkAllAsAlert(
      UserNotificationMarkAlertDto dto
    )
    {
      try
      {
        var client = CreateClient();
        var response = await client.PatchAsJsonAsync(
          "user-notifications/mark/alert/all",
          dto,
          JsonOptions
        );
        return await response.Content.ReadFromJsonAsync<IEnumerable<UserNotificationDto>>(
            JsonOptions
          ) ?? [];
      }
      catch
      {
        return [];
      }
    }
  }
}
