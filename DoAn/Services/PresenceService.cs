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
        private Task? _pingLoopTask;
        private string _activeBaseUrl;

        private static readonly string[] BaseUrls =
        [
            "http://vinh-khanh.somee.com/api/",
            "http://10.0.2.2:5143/api/",
            "http://localhost:5143/api/",
            "http://10.93.119.86:5143/api/"
        ];

        private PresenceService()
        {
            _activeBaseUrl = NormalizeBaseUrl(Preferences.Get("ApiBaseUrl", "http://vinh-khanh.somee.com/api/"));
            _sessionId = Preferences.Get("AnonymousSessionId", "");
            if (string.IsNullOrWhiteSpace(_sessionId))
            {
                _sessionId = Guid.NewGuid().ToString("N");
                Preferences.Set("AnonymousSessionId", _sessionId);
            }
        }

        public string SessionId => _sessionId;

        public void SetPreferredBaseUrl(string? apiBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl)) return;

            _activeBaseUrl = NormalizeBaseUrl(apiBaseUrl);
            Preferences.Set("ApiBaseUrl", _activeBaseUrl);
        }

        public void Start()
        {
            if (_pingLoopTask is { IsCompleted: false })
            {
                _ = RefreshAsync();
                return;
            }

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pingLoopTask = Task.Run(() => PingLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _pingLoopTask = null;
        }

        public Task RefreshAsync() => PingOnceAsync();

        private async Task PingLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await PingOnceAsync();

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            finally
            {
                _pingLoopTask = null;
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

        private static string NormalizeBaseUrl(string value)
        {
            var normalized = value.Trim();
            if (!normalized.EndsWith('/'))
                normalized += "/";

            return normalized;
        }
    }
}

