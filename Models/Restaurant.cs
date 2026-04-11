namespace DoAn.FRONTEND.Models
{
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

        public string GetDescription(string lang) => lang switch
        {
            "en" => Description_en ?? Description_vi ?? "",
            "ko" => Description_kr ?? Description_vi ?? "",
            "zh" => Description_cn ?? Description_vi ?? "",
            _ => Description_vi ?? ""
        };

        public string GetAudioContent(string lang) => lang switch
        {
            "en" => AudioContent_en ?? AudioContent_vi ?? Name ?? "",
            "ko" => AudioContent_kr ?? AudioContent_vi ?? Name ?? "",
            "zh" => AudioContent_cn ?? AudioContent_vi ?? Name ?? "",
            _ => AudioContent_vi ?? Name ?? ""
        };
    }
}