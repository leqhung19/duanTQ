using DoAn.Services;

namespace DoAn
{
    public partial class App : Application
    {
        [Obsolete]
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();

            // Lắng nghe sự kiện POI
            POIService.Instance.OnPOIEntered += async (poi) =>
            {
                await POITriggerService.Instance.TriggerPOIAsync(
                    poi,
                    POIService.Instance.CurrentLang
                );
            };
        }
    }
}