using DoAn.FRONTEND.Models;
using DoAn.Services;

namespace DoAn.Views
{
    [QueryProperty(nameof(CurrentRestaurant), "Restaurant")]
    public partial class DetailPage : ContentPage
    {
        private Restaurant? _currentRestaurant;
        private string _selectedLang = "vi";

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

            RestaurantImage.Source = _currentRestaurant.Image;
            NameLabel.Text = _currentRestaurant.Name;
            OpenTimeLabel.Text = _currentRestaurant.OpenTime ?? "Chưa cập nhật";
            AddressLabel.Text = _currentRestaurant.Address ?? "Chưa cập nhật";
            PhoneLabel.Text = _currentRestaurant.Phone ?? "Chưa cập nhật";
            PriceRangeLabel.Text = _currentRestaurant.PriceRange ?? "Liên hệ";

            // Đồng bộ ngôn ngữ với POIService
            _selectedLang = POIService.Instance.CurrentLang;
            UpdateLangButtons(_selectedLang);
            UpdateDescription();
        }

        // ============ NGÔN NGỮ ============
        private void OnVietnameseClicked(object? sender, EventArgs e)
        { _selectedLang = "vi"; Sync(); }
        private void OnEnglishClicked(object? sender, EventArgs e)
        { _selectedLang = "en"; Sync(); }
        private void OnKoreanClicked(object? sender, EventArgs e)
        { _selectedLang = "ko"; Sync(); }
        private void OnChineseClicked(object? sender, EventArgs e)
        { _selectedLang = "zh"; Sync(); }

        private void Sync()
        {
            POIService.Instance.CurrentLang = _selectedLang;
            UpdateLangButtons(_selectedLang);
            UpdateDescription();
        }

        private void UpdateLangButtons(string lang)
        {
            var active = Color.FromArgb("#FF5722");
            var inactive = Color.FromArgb("#E0E0E0");
            var white = Colors.White;
            var dark = Color.FromArgb("#212121");

            BtnVi.BackgroundColor = lang == "vi" ? active : inactive;
            BtnEn.BackgroundColor = lang == "en" ? active : inactive;
            BtnKr.BackgroundColor = lang == "ko" ? active : inactive;
            BtnCn.BackgroundColor = lang == "zh" ? active : inactive;

            BtnVi.TextColor = lang == "vi" ? white : dark;
            BtnEn.TextColor = lang == "en" ? white : dark;
            BtnKr.TextColor = lang == "ko" ? white : dark;
            BtnCn.TextColor = lang == "zh" ? white : dark;
        }

        private void UpdateDescription()
        {
            if (_currentRestaurant == null) return;
            DescriptionLabel.Text = _currentRestaurant.GetDescription(_selectedLang);
        }

        private async void OnPlayAudioClicked(object? sender, EventArgs e)
        {
            if (_currentRestaurant == null) return;
            await POITriggerService.Instance.TriggerPOIAsync(_currentRestaurant, _selectedLang);
        }

        private async void OnViewMapClicked(object? sender, EventArgs e)
        {
            if (_currentRestaurant == null) return;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            var route = $"{nameof(RestaurantMapPage)}" +
                        $"?Lat={_currentRestaurant.Latitude.ToString(ci)}" +
                        $"&Lng={_currentRestaurant.Longitude.ToString(ci)}" +
                        $"&Name={Uri.EscapeDataString(_currentRestaurant.Name ?? "")}";
            await Shell.Current.GoToAsync(route);
        }
    }
}