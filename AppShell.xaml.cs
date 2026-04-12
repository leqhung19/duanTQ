using DoAn.Views;

namespace DoAn
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
            Routing.RegisterRoute(nameof(RestaurantMapPage), typeof(RestaurantMapPage));
        }
    }
}