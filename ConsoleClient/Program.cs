using System;
using ApiClient;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class Program
    {
        const string loginSessionFilePath = ".loginSession";

        static List<BussinessesItem> lastStatus = new List<BussinessesItem>();

        static HttpClient client = new HttpClient();


        static void Main(string[] args)
        {
            var apiClient = new ApiClient.ApiClient();

                client.BaseAddress = new Uri("https://maker.ifttt.com");


            LoginSession loginSession = null;
            if (!File.Exists(loginSessionFilePath))
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

            var items = apiClient.ListFavoriteBusinesses(loginSession).Result;

            foreach (var i in items)
            {
                Console.WriteLine($"{i.DisplayName} - {i.ItemsAvailable}");

                var previousStatus = lastStatus.FirstOrDefault(x => x.Item.Id == i.Item.Id);

                if( previousStatus != null && 
                    previousStatus.ItemsAvailable == 0 &&
                    i.ItemsAvailable > 0)
                    SendNotification(i).Wait();
            }

            lastStatus = items;
        }

        private static async Task SendNotification(BussinessesItem i)
        {
            var result = await client.GetAsync($"/trigger/tgtg_available/with/key/dvmAm_M6vu6EGrIO9tRsxA?value1={i.DisplayName}&value2={i.ItemsAvailable}");
        }

        static void UpdateLoginSessionFile(LoginSession loginSession)
        {
            var loginSessionJson = JsonSerializer.Serialize(loginSession);
            File.WriteAllText(loginSessionFilePath, loginSessionJson);
        }
    }
}
