using DoAn;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using DoAn.Views;
using ZXing.Net.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace DoAn
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()    // Initializes ZXing.Net.MAUI.Controls
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register Views for routing and dependency injection
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<DetailPage>();
            builder.Services.AddTransient<MapPage>();
            builder.Services.AddTransient<QRPage>();

            return builder.Build();
        }
    }

}