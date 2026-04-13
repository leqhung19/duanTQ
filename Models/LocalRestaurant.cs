using SQLite;

namespace DoAn.FRONTEND.Models
{
    [Table("Restaurants")]
    public class LocalRestaurant
    {
        [PrimaryKey]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; } = "";

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

        public string GetDescription(string lang) => lang switch
        {
            "en" => Description_en ?? Description_vi ?? "",
            "ko" => Description_kr ?? Description_vi ?? "",
            "zh" => Description_cn ?? Description_vi ?? "",
            _ => Description_vi ?? ""
        };

        public string GetAudioContent(string lang) => lang switch
        {
            "en" => AudioContent_en ?? AudioContent_vi ?? Name,
            "ko" => AudioContent_kr ?? AudioContent_vi ?? Name,
            "zh" => AudioContent_cn ?? AudioContent_vi ?? Name,
            _ => AudioContent_vi ?? Name
        };

        // Convert sang Restaurant model
        public Restaurant ToRestaurant() => new()
        {
            Id = Id,
            Name = Name,
            Image = Image,
            Description_vi = Description_vi,
            Description_en = Description_en,
            Description_kr = Description_kr,
            Description_cn = Description_cn,
            Address = Address,
            Phone = Phone,
            OpenTime = OpenTime,
            PriceRange = PriceRange,
            Latitude = Latitude,
            Longitude = Longitude,
            RadiusMeters = RadiusMeters,
            Priority = Priority,
            AudioContent_vi = AudioContent_vi,
            AudioContent_en = AudioContent_en,
            AudioContent_kr = AudioContent_kr,
            AudioContent_cn = AudioContent_cn
        };
    }

    [Table("QRCodes")]
    public class LocalQRCode
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        [NotNull]
        public string QRContent { get; set; } = "";
    }
}