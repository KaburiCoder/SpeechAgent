using SpeechAgent.Services.MedicSIO.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpeechAgent.Services.Api
{
  public interface ILlmApi
  {
    Task<PatientInfoDto> GetPatientInfoByImage(string imageUrl);
  }

  internal class LlmApi : ILlmApi
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public LlmApi(IHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
      _jsonOptions = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };
    }

    public async Task<PatientInfoDto> GetPatientInfoByImage(string imageUrl)
    {
      try
      {
        var client = _httpClientFactory.CreateClient("SpeechServer");
        var request = new GetPatientInfoByImageRequestDto() { ImageUrl = imageUrl };
        var response = await client.PostAsJsonAsync("llm/get-patient-info-by-image", request, _jsonOptions);
        var patientInfo = await response.Content.ReadFromJsonAsync<PatientInfoDto>(_jsonOptions);
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
    public string ImageUrl { get; set; }
  }
}
