using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace VinhKhanh.Admin.Services;

// Dùng MyMemory API — miễn phí, không cần key
public class TranslationService
{
    private readonly HttpClient _http;
    private readonly ILogger<TranslationService> _log;

    public TranslationService(ILogger<TranslationService> log)
    {
        _http = new HttpClient();
        _log = log;
    }

    // Dịch text từ ngôn ngữ nguồn sang đích
    // langPair: "vi|en", "vi|zh", "en|vi"...
    public async Task<string?> TranslateAsync(string text, string langPair)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        try
        {
            var url = $"https://api.mymemory.translated.net/get" +
                      $"?q={Uri.EscapeDataString(text)}&langpair={langPair}";
            var resp = await _http.GetStringAsync(url);
            var doc = JsonDocument.Parse(resp);
            return doc.RootElement
                .GetProperty("responseData")
                .GetProperty("translatedText")
                .GetString();
        }
        catch (Exception ex)
        {
            _log.LogWarning("Dịch lỗi ({Pair}): {Msg}", langPair, ex.Message);
            return null;
        }
    }

    // Từ 1 ngôn ngữ đã có, tự dịch sang 2 ngôn ngữ còn lại
    public async Task AutoFillTranslationsAsync(
        string? vi, string? en, string? cn,
        Action<string> setVi, Action<string> setEn, Action<string> setCn)
    {
        // Xác định nguồn
        string? source = null;
        string srcLang = "";
        if (!string.IsNullOrWhiteSpace(vi)) { source = vi; srcLang = "vi"; }
        else if (!string.IsNullOrWhiteSpace(en)) { source = en; srcLang = "en"; }
        else if (!string.IsNullOrWhiteSpace(cn)) { source = cn; srcLang = "zh-CN"; }

        if (source is null) return;

        if (string.IsNullOrWhiteSpace(vi) && srcLang != "vi")
        {
            var t = await TranslateAsync(source, $"{srcLang}|vi");
            if (t is not null) setVi(t);
        }
        if (string.IsNullOrWhiteSpace(en) && srcLang != "en")
        {
            var t = await TranslateAsync(source, $"{srcLang}|en");
            if (t is not null) setEn(t);
        }
        if (string.IsNullOrWhiteSpace(cn) && srcLang != "zh-CN")
        {
            var t = await TranslateAsync(source, $"{srcLang}|zh-CN");
            if (t is not null) setCn(t);
        }
    }
}