using DoAn.FRONTEND.Models;

namespace DoAn.Services
{
    public class POITriggerService
    {
        private static POITriggerService? _instance;
        public static POITriggerService Instance => _instance ??= new POITriggerService();

        public async Task TriggerPOIAsync(Restaurant poi, string lang, string triggerType = "gps")
        {
            try
            {
                await VibrateAsync();
                await Task.Delay(500);
                var audioSource = await PlayPublishedAudioAsync(poi, lang)
                    ? "file"
                    : await SpeakAsync(poi, lang);

                await RestaurantService.Instance.LogListenAsync(poi.Id, lang, triggerType, audioSource);
            }
            catch { }
        }

        public async Task SpeakDescriptionAsync(Restaurant poi, string lang)
        {
            try
            {
                await VibrateAsync();
                var text = poi.GetDescription(lang);
                await SpeakTextAsync(text, lang);
                await RestaurantService.Instance.LogListenAsync(poi.Id, lang, "manual", "tts");
            }
            catch { }
        }

        private async Task VibrateAsync()
        {
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
                await Task.Delay(300);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(400));
                await Task.Delay(500);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
            }
            catch { }
        }

        private async Task<bool> PlayPublishedAudioAsync(Restaurant poi, string lang)
        {
            try
            {
                var audio = poi.AudioFiles.FirstOrDefault(a =>
                                string.Equals(a.Language, lang, StringComparison.OrdinalIgnoreCase))
                            ?? poi.AudioFiles.FirstOrDefault(a =>
                                string.Equals(a.Language, "vi", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrWhiteSpace(audio?.Url)) return false;

                await Launcher.Default.OpenAsync(audio.Url);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> SpeakAsync(Restaurant poi, string lang)
        {
            try
            {
                var text = poi.GetAudioContent(lang);
                await SpeakTextAsync(text, lang);
            }
            catch { }

            return "tts";
        }

        private async Task SpeakTextAsync(string? text, string lang)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var locale = lang switch
            {
                "en" => locales.FirstOrDefault(l => l.Language.StartsWith("en")),
                "ko" => locales.FirstOrDefault(l => l.Language.StartsWith("ko")),
                "zh" => locales.FirstOrDefault(l => l.Language.StartsWith("zh")),
                _ => locales.FirstOrDefault(l => l.Language.StartsWith("vi"))
            };

            await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
            {
                Locale = locale,
                Volume = 1.0f,
                Pitch = 1.0f
            });
        }
    }
}
