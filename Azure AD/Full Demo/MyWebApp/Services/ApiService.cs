using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

using Newtonsoft.Json.Linq;

namespace MyWebApp.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;

        public ApiService(IHttpClientFactory clientFactory,
                          ITokenAcquisition tokenAcquisition,
                          IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
        }

        public async Task<JArray> GetApiDataAsync()
        {
            var client = await GetHttpClientAsync();
            var response = await client.GetAsync("api/weatherforecast");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JArray.Parse(responseContent);

                return data;
            }

            //TODO: logging and filter to catch these execptions to give the user a correct fault message
            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");

        }

        public async Task<JArray> GetApiDataUsersAsync()
        {
            var client = await GetHttpClientAsync();
            var response = await client.GetAsync("api/users");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JArray.Parse(responseContent);

                return data;
            }

            //TODO: logging and filter to catch these execptions to give the user a correct fault message
            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");

        }

        private async Task<HttpClient> GetHttpClientAsync()
        {
            var client = _clientFactory.CreateClient();

            var scopes = _configuration.GetSection("Api:ScopesForAccessToken").Get<string[]>();
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            client.BaseAddress = new Uri(_configuration["Api:ApiBaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}
