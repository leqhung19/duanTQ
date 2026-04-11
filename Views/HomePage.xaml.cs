using DoAn.FRONTEND.Models;
using DoAn.Services;

namespace DoAn.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Fetch data from Mock API
            var restaurants = await MockDataService.GetRestaurantsAsync();
            RestaurantsCollection.ItemsSource = restaurants;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private async void OnRestaurantSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Restaurant selectedRestaurant)
            {
                // Navigate to DetailPage and pass the object
                var navigationParameter = new Dictionary<string, object>
                {
                    { "Restaurant", selectedRestaurant }
                };

                await Shell.Current.GoToAsync(nameof(DetailPage), navigationParameter);

                // Deselect item
                if (sender is CollectionView collectionView)
                {
                    collectionView.SelectedItem = null;
                }
            }
        }
    }
}