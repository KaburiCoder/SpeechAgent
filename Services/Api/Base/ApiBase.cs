using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpeechAgent.Services.Api
{
  public class ApiBase
  {
    readonly IHttpClientFactory _httpClientFactory;
    readonly string _clientName = string.Empty;

    protected readonly JsonSerializerOptions JsonOptions;

    public ApiBase(IHttpClientFactory httpClientFactory, string clientName = "SpeechServer")
    {
      _httpClientFactory = httpClientFactory;
      JsonOptions = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      };
      _clientName = clientName;
    }

    protected HttpClient CreateClient()
    {
      return _httpClientFactory.CreateClient(_clientName);
    }
  }
}
