using System;
using ApiClient;
using System.Text.Json;
using System.IO;

namespace ConsoleClient
{
    class Program
    {
        const string loginSessionFilePath = ".loginSession";

        static void Main(string[] args)
        {
            var apiClient = new ApiClient.ApiClient();

            LoginSession loginSession = null;
            if(!File.Exists(loginSessionFilePath))
            {
                loginSession = apiClient.LoginByEmail("siven_7+tgtgtest@hotmail.com", "jbrFTGe47PpieFpj").Result;
                UpdateLoginSessionFile(loginSession);
            }
            else
            {
                var loginSessionJson = File.ReadAllText(loginSessionFilePath);
                loginSession = JsonSerializer.Deserialize<LoginSession>(loginSessionJson);

                apiClient.RefreshToken(loginSession).Wait();
                UpdateLoginSessionFile(loginSession);
            }

            apiClient.ListFavoriteBusinesses(loginSession).Wait();
        }

        static void UpdateLoginSessionFile(LoginSession loginSession)
        {
            var loginSessionJson = JsonSerializer.Serialize(loginSession);
            File.WriteAllText(loginSessionFilePath, loginSessionJson);
        }
    }
}
