using DoAn.FRONTEND.Models;
using DoAn.Services;

namespace DoAn.Views
{
    public partial class HomePage : ContentPage
    {
        private List<Restaurant> _allRestaurants = new();

        public HomePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
            await StartPOITrackingAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Không stop POI khi chuyển tab
        }

        private async Task LoadDataAsync()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            _allRestaurants = await RestaurantService.Instance.GetAllAsync();
            RestaurantCollection.ItemsSource = _allRestaurants;

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }

        private async Task StartPOITrackingAsync()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted) return;

            POIService.Instance.SetPOIs(_allRestaurants);
            _ = POIService.Instance.StartTrackingAsync();
        }

        // Tìm kiếm
        private void OnSearchChanged(object? sender, TextChangedEventArgs e)
        {
            var keyword = e.NewTextValue?.ToLower() ?? "";
            RestaurantCollection.ItemsSource = string.IsNullOrEmpty(keyword)
                ? _allRestaurants
                : _allRestaurants.Where(r =>
                    (r.Name?.ToLower().Contains(keyword) ?? false) ||
                    (r.Address?.ToLower().Contains(keyword) ?? false)).ToList();
        }

        // Chọn quán
        private async void OnRestaurantSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not Restaurant selected) return;
            (sender as CollectionView)!.SelectedItem = null;

            await Shell.Current.GoToAsync(nameof(DetailPage),
                new Dictionary<string, object> { { "Restaurant", selected } });
        }

        // Pull to refresh
        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await LoadDataAsync();
            RefreshView.IsRefreshing = false;
        }
    }
}