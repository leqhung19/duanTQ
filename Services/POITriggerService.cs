using DoAn.FRONTEND.Models;

namespace DoAn.Services
{
    public class POITriggerService
    {
        private static POITriggerService? _instance;
        public static POITriggerService Instance => _instance ??= new POITriggerService();

        public async Task TriggerPOIAsync(Restaurant poi, string lang)
        {
            try
            {
                await VibrateAsync();
                await Task.Delay(500);
                await SpeakAsync(poi, lang);
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

        private async Task SpeakAsync(Restaurant poi, string lang)
        {
            try
            {
                var text = poi.GetAudioContent(lang);
                if (string.IsNullOrEmpty(text)) return;

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
            catch { }
        }
    }
}