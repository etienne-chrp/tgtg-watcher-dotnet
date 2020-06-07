using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ApiClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TimedHostedService : IHostedService, IDisposable
{
    private readonly ILogger _logger;
    private Timer _timerSession;
    private Timer _timerItems;

    const string loginSessionFilePath = ".loginSession";
    private List<BussinessesItem> lastStatus = new List<BussinessesItem>();
    private HttpClient iftttClient = new HttpClient()
    {
        BaseAddress = new Uri("https://maker.ifttt.com")
    };
    private ApiClient.ApiClient apiClient = new ApiClient.ApiClient();
    private LoginSession loginSession = null;

    public TimedHostedService(ILogger<TimedHostedService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service is starting.");

        RefreshSession(null);
        _timerSession = new Timer(RefreshSession, null, TimeSpan.Zero, TimeSpan.FromSeconds(3600));
        _timerItems = new Timer(CheckItems, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        return Task.CompletedTask;
    }

    private void CheckItems(object state)
    {
        var items = apiClient.ListFavoriteBusinesses(loginSession).Result;

        foreach (var i in items)
        {
            Console.WriteLine($"{i.DisplayName} - {i.ItemsAvailable}");

            var previousStatus = lastStatus.FirstOrDefault(x => x.Item.Id == i.Item.Id);

            if (previousStatus != null &&
                previousStatus.ItemsAvailable == 0 &&
                i.ItemsAvailable > 0)
                SendNotification(i).Wait();
        }

        lastStatus = items;
    }

    private void RefreshSession(object state)
    {
        _logger.LogInformation("Service is running.");

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
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service is stopping.");

        _timerSession?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timerSession?.Dispose();
    }

    private async Task SendNotification(BussinessesItem i)
    {
        var result = await iftttClient.GetAsync($"/trigger/tgtg_available/with/key/dvmAm_M6vu6EGrIO9tRsxA?value1={i.DisplayName}&value2={i.ItemsAvailable}");
    }    

    private void UpdateLoginSessionFile(LoginSession loginSession)
    {
        var loginSessionJson = JsonSerializer.Serialize(loginSession);
        File.WriteAllText(loginSessionFilePath, loginSessionJson);
    }
}