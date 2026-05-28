using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SpeechAgent.Services.MedicSIO.Dto;
using SpeechAgent.Utils;

namespace SpeechAgent.Services.Api
{
  public interface ILlmApi
  {
    Task<PatientInfoDto> GetPatientInfoByImage(string imageUrl);
  }

  internal class LlmApi : ApiBase, ILlmApi
  {
    public LlmApi(IHttpClientFactory httpClientFactory) : base(httpClientFactory, "SpeechServer")
    {
    }

    public async Task<PatientInfoDto> GetPatientInfoByImage(string imageUrl)
    {
      HttpResponseMessage? response = null;
      try
      {
        var client = CreateClient();
        var request = new GetPatientInfoByImageRequestDto() { ImageUrl = imageUrl };
        LogUtils.WriteLog(
          LogLevel.Debug,
          $"[LlmApi] GetPatientInfoByImage 호출 시도 (baseUrl={client.BaseAddress}, imageLen={imageUrl?.Length ?? 0})"
        );

        response = await client.PostAsJsonAsync(
          "llm/get-patient-info-by-image",
          request,
          JsonOptions
        );

        if (!response.IsSuccessStatusCode)
        {
          // 401/400/500 등은 throw하지 않으므로 명시적으로 분기 — body까지 남겨야 인증/스키마 오류 진단 가능.
          var body = await response.Content.ReadAsStringAsync();
          LogUtils.WriteLog(
            LogLevel.Error,
            $"[LlmApi] GetPatientInfoByImage 비정상 응답 (statusCode={(int)response.StatusCode} {response.StatusCode}, body={body})"
          );
          return new PatientInfoDto { Chart = "", Name = "" };
        }

        var patientInfo = await response.Content.ReadFromJsonAsync<PatientInfoDto>(JsonOptions);
        LogUtils.WriteLog(
          LogLevel.Debug,
          $"[LlmApi] GetPatientInfoByImage 성공 (chart='{patientInfo?.Chart}', name='{patientInfo?.Name}')"
        );
        return patientInfo!;
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(
          LogLevel.Error,
          $"[LlmApi] GetPatientInfoByImage 예외 (statusCode={(int?)response?.StatusCode})",
          ex
        );
        return new PatientInfoDto { Chart = "", Name = "" };
      }
    }
  }

  public class GetPatientInfoByImageRequestDto
  {
    public string ImageUrl { get; set; } = "";
  }
}
