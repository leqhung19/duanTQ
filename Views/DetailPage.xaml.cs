using DoAn.FRONTEND.Models;

namespace DoAn.Views
{
    [QueryProperty(nameof(CurrentRestaurant), "Restaurant")]
    public partial class DetailPage : ContentPage
    {
        private Restaurant? _currentRestaurant;
        private string _selectedLang = "vi"; // Default language

        // This is the property that was causing the error
        public Restaurant? CurrentRestaurant
        {
            get => _currentRestaurant;
            set
            {
                _currentRestaurant = value;
                OnPropertyChanged();
                LoadUI();
            }
        }

        public DetailPage()
        {
            InitializeComponent();
        }

        private void LoadUI()
        {
            if (_currentRestaurant == null) return;

            // Set the image and title
            if (RestaurantImage != null)
                RestaurantImage.Source = _currentRestaurant.Image;

            if (NameLabel != null)
                NameLabel.Text = _currentRestaurant.Name;

            UpdateDescription();
        }

        // Language Button Events
        private void OnVietnameseClicked(object? sender, EventArgs e) { _selectedLang = "vi"; UpdateDescription(); }
        private void OnEnglishClicked(object? sender, EventArgs e) { _selectedLang = "en"; UpdateDescription(); }
        private void OnKoreanClicked(object? sender, EventArgs e) { _selectedLang = "ko"; UpdateDescription(); }
        private void OnChineseClicked(object? sender, EventArgs e) { _selectedLang = "zh"; UpdateDescription(); }

        private void UpdateDescription()
        {
            if (_currentRestaurant == null || DescriptionLabel == null) return;

            // Switch text based on selected language
            DescriptionLabel.Text = _selectedLang switch
            {
                "en" => _currentRestaurant.Description_en,
                "ko" => _currentRestaurant.Description_kr,
                "zh" => _currentRestaurant.Description_cn,
                _ => _currentRestaurant.Description_vi
            } ?? "No description available.";
        }

        private async void OnPlayAudioClicked(object? sender, EventArgs e)
        {
            if (DescriptionLabel == null || string.IsNullOrEmpty(DescriptionLabel.Text)) return;

            var locales = await TextToSpeech.Default.GetLocalesAsync();

            // Find the correct voice for the selected language
            var locale = _selectedLang switch
            {
                "en" => locales.FirstOrDefault(l => l.Language.StartsWith("en")),
                "ko" => locales.FirstOrDefault(l => l.Language.StartsWith("ko")),
                "zh" => locales.FirstOrDefault(l => l.Language.StartsWith("zh")),
                _ => locales.FirstOrDefault(l => l.Language.StartsWith("vi"))
            };

            await TextToSpeech.Default.SpeakAsync(DescriptionLabel.Text, new SpeechOptions { Locale = locale });
        }
        // Trong DetailPage.xaml.cs
        public void LoadMockData(int id)
        {
            var mockList = new List<Restaurant>
    {
        new Restaurant { Id = 101, Name = "Bánh Mì Mock", Description_vi = "Dữ liệu giả để test QR 101" },
        new Restaurant { Id = 102, Name = "Cơm Tấm Mock", Description_vi = "Dữ liệu giả để test QR 102" }
    };

            var restaurant = mockList.FirstOrDefault(r => r.Id == id);
            if (restaurant != null)
            {
                BindingContext = restaurant;
            }
        }
        private async void OnViewMapClicked(object? sender, EventArgs e)
        {
            if (_currentRestaurant == null) return;

            var route = $"//MapPage?Lat={_currentRestaurant.Latitude}&Lng={_currentRestaurant.Longitude}&Name={Uri.EscapeDataString(_currentRestaurant.Name ?? "")}";
            await Shell.Current.GoToAsync(route);
        }
    }
}