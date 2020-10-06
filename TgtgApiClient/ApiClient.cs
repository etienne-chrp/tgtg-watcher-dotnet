using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ApiClient
{
    public class ApiClient
    {
        const string baseUrl = "https://apptoogoodtogo.com/";

        private readonly ILogger _logger;
        private HttpClient client = new HttpClient();

        public ApiClient(ILogger logger)
        {
            _logger = logger;

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
            client.DefaultRequestHeaders.Add("User-Agent", "TGTG/20.9.1 (317) (Android/Unknown; Scale/3.00)");
        }

        private async Task<HttpResponseMessage> PostJsonAsync(string path, string jsonContent, string authToken = null)
        {
            if (authToken != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var postTask = client.PostAsync(path, new StringContent(jsonContent, Encoding.UTF8, "application/json"));

            var msg = await postTask;
            var content = await msg.Content.ReadAsStringAsync();

            try
            {
                msg.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException http_ex)
            {
                _logger.LogError($"{content}\n{http_ex}");
                throw;
            }

            return msg;
        }

        public async Task<LoginSession> LoginByEmail(string email, string password)
        {
            var result = await PostJsonAsync(
                "/api/auth/v1/loginByEmail",
                $@"{{ 
                        ""device_type"": ""UNKNOWN"",
                        ""email"": ""{email}"",
                        ""password"": ""{password}"" 
                    }}");

            var loginSession = await JsonSerializer.DeserializeAsync<LoginSession>(await result.Content.ReadAsStreamAsync());
            return loginSession;
        }

        public async Task RefreshToken(LoginSession loginSession)
        {
            var result = await PostJsonAsync(
                "/api/auth/v1/token/refresh",
                $@"{{
                ""refresh_token"": ""{loginSession.RefreshToken}""
            }}");

            var newToken = await JsonSerializer.DeserializeAsync<LoginSession>(await result.Content.ReadAsStreamAsync());
            loginSession.AccessToken = newToken.AccessToken;
        }

        public async Task<List<BussinessesItem>> ListFavoriteBusinesses(LoginSession loginSession)
        {
            var result = await PostJsonAsync(
                "/api/item/v4/",
                $@"{{
                    ""favorites_only"": true,
                    ""origin"": {{
                        ""latitude"": 52.5170365,
                        ""longitude"": 13.3888599
                    }},
                    ""radius"": ""200"",
                    ""user_id"": ""{loginSession.Data.UserInfo.Id}""
                }}",
                loginSession.AccessToken);

            var items = await JsonSerializer.DeserializeAsync<BussinessesItemsResponse>(await result.Content.ReadAsStreamAsync());
            return items.BusinessesItems;
        }
    }
}
