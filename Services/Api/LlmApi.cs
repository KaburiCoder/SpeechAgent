using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SpeechAgent.Services.MedicSIO.Dto;

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
      try
      {
        var client = CreateClient();
        var request = new GetPatientInfoByImageRequestDto() { ImageUrl = imageUrl };
        var response = await client.PostAsJsonAsync(
          "llm/get-patient-info-by-image",
          request,
          JsonOptions
        );
        var patientInfo = await response.Content.ReadFromJsonAsync<PatientInfoDto>(JsonOptions);
        return patientInfo!;
      }
      catch
      {
        return new PatientInfoDto { Chart = "", Name = "" };
      }
    }
  }

  public class GetPatientInfoByImageRequestDto
  {
    public string ImageUrl { get; set; } = "";
  }
}
