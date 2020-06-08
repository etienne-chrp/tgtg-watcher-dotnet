using System;
using System.Collections;
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
using Microsoft.Extensions.Options;

namespace TgtgWatcherService
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private IOptions<AppConfig> _appConfig;
        private Timer _timerSession;
        private Timer _timerItems;

        const string loginSessionFilePath = ".loginSession";
        private List<BussinessesItem> lastStatus = new List<BussinessesItem>();
        private HttpClient iftttClient = new HttpClient()
        {
            BaseAddress = new Uri("https://maker.ifttt.com")
        };
        private ApiClient.ApiClient _apiClient;
        private LoginSession loginSession = null;

        public TimedHostedService(ILogger<TimedHostedService> logger, IOptions<AppConfig> appConfig)
        {
            _logger = logger;
            _appConfig = appConfig;

            _apiClient = new ApiClient.ApiClient(_logger);
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
            var items = _apiClient.ListFavoriteBusinesses(loginSession).Result;

            foreach (var i in items)
            {
                _logger.LogDebug($"{i.DisplayName} - {i.ItemsAvailable}");

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

            foreach(DictionaryEntry env in Environment.GetEnvironmentVariables())
                _logger.LogWarning($"{env.Key}={env.Value}");

            if (!File.Exists(loginSessionFilePath))
            {
                loginSession = _apiClient.LoginByEmail(
                    _appConfig.Value.TgtgUsername,
                    _appConfig.Value.TgtgPassword
                ).Result;
                UpdateLoginSessionFile(loginSession);
            }
            else
            {
                var loginSessionJson = File.ReadAllText(loginSessionFilePath);
                loginSession = JsonSerializer.Deserialize<LoginSession>(loginSessionJson);

                _apiClient.RefreshToken(loginSession).Wait();
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
            var result = await iftttClient.GetAsync($"{_appConfig.Value.IftttTriggerUrl}?value1={i.DisplayName}&value2={i.ItemsAvailable}");
        }

        private void UpdateLoginSessionFile(LoginSession loginSession)
        {
            var loginSessionJson = JsonSerializer.Serialize(loginSession);
            File.WriteAllText(loginSessionFilePath, loginSessionJson);
        }
    }
}