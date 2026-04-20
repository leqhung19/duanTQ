using System.Net.Http.Json;

namespace DoAn.Services
{
    public class PresenceService
    {
        private static PresenceService? _instance;
        public static PresenceService Instance => _instance ??= new PresenceService();

        private readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        private readonly string _sessionId;
        private CancellationTokenSource? _cts;
        private string _activeBaseUrl = "http://10.0.2.2:5143/api/";

        private static readonly string[] BaseUrls =
        [
            "http://10.0.2.2:5143/api/",
            "http://localhost:5143/api/",
            "http://10.93.119.86:5143/api/"
        ];

        private PresenceService()
        {
            _sessionId = Preferences.Get("AnonymousSessionId", "");
            if (string.IsNullOrWhiteSpace(_sessionId))
            {
                _sessionId = Guid.NewGuid().ToString("N");
                Preferences.Set("AnonymousSessionId", _sessionId);
            }
        }

        public void Start()
        {
            if (_cts is not null) return;

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => PingLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }

        private async Task PingLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await PingOnceAsync();

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task PingOnceAsync()
        {
            var payload = new
            {
                sessionId = _sessionId,
                platform = DeviceInfo.Platform.ToString().ToLowerInvariant(),
                language = POIService.Instance.CurrentLang
            };

            foreach (var baseUrl in BaseUrls.Prepend(_activeBaseUrl).Distinct())
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(baseUrl + "sync/session/ping", payload);
                    if (response.IsSuccessStatusCode)
                    {
                        _activeBaseUrl = baseUrl;
                        return;
                    }
                }
                catch
                {
                    // Thu base URL tiep theo.
                }
            }
        }
    }
}
