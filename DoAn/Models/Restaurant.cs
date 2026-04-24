namespace DoAn.FRONTEND.Models
{
    public class SyncResponse
    {
        public long Timestamp { get; set; }
        public int Count { get; set; }
        public List<SyncRestaurant> Pois { get; set; } = new();
    }

    public class SyncRestaurant
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public LocalizedText? Description { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? OpenTime { get; set; }
        public string? PriceRange { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusMeters { get; set; } = 30;
        public int Priority { get; set; } = 1;
        public LocalizedText? AudioText { get; set; }
        public List<RestaurantAudioFile> AudioFiles { get; set; } = new();
        public List<string> QrCodes { get; set; } = new();
        public DateTime UpdatedAt { get; set; }

        public Restaurant ToRestaurant() => new()
        {
            Id = Id,
            CategoryId = CategoryId ?? 0,
            Name = Name,
            Image = ImageUrl,
            Description_vi = Description?.Vi,
            Description_en = Description?.En,
            Description_kr = Description?.Ko,
            Description_cn = Description?.Cn,
            Address = Address,
            Phone = Phone,
            OpenTime = OpenTime,
            PriceRange = PriceRange,
            Latitude = Latitude,
            Longitude = Longitude,
            RadiusMeters = RadiusMeters,
            Priority = Priority,
            AudioContent_vi = AudioText?.Vi,
            AudioContent_en = AudioText?.En,
            AudioContent_kr = AudioText?.Ko,
            AudioContent_cn = AudioText?.Cn,
            IsActive = true,
            AudioFiles = AudioFiles,
            QrCodes = QrCodes
        };
    }

    public class LocalizedText
    {
        public string? Vi { get; set; }
        public string? En { get; set; }
        public string? Ko { get; set; }
        public string? Cn { get; set; }
    }

    public class RestaurantAudioFile
    {
        public int Id { get; set; }
        public string? Language { get; set; }
        public string? Url { get; set; }
        public long FileSizeBytes { get; set; }
    }

    public class Restaurant
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? Description_vi { get; set; }
        public string? Description_en { get; set; }
        public string? Description_kr { get; set; }
        public string? Description_cn { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? OpenTime { get; set; }
        public string? PriceRange { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusMeters { get; set; } = 30;
        public int Priority { get; set; } = 1;
        public string? AudioContent_vi { get; set; }
        public string? AudioContent_en { get; set; }
        public string? AudioContent_kr { get; set; }
        public string? AudioContent_cn { get; set; }
        public bool IsActive { get; set; } = true;
        public List<RestaurantAudioFile> AudioFiles { get; set; } = new();
        public List<string> QrCodes { get; set; } = new();

        public string GetDescription(string lang) => lang switch
        {
            "en" => Description_en ?? Description_vi ?? "",
            "ko" => Description_kr ?? Description_vi ?? "",
            "zh" or "cn" => Description_cn ?? Description_vi ?? "",
            _ => Description_vi ?? ""
        };

        public string GetAudioContent(string lang) => lang switch
        {
            "en" => AudioContent_en ?? AudioContent_vi ?? Name ?? "",
            "ko" => AudioContent_kr ?? AudioContent_vi ?? Name ?? "",
            "zh" or "cn" => AudioContent_cn ?? AudioContent_vi ?? Name ?? "",
            _ => AudioContent_vi ?? Name ?? ""
        };
    }
}
