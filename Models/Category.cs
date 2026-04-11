namespace DoAn.FRONTEND.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name_vi { get; set; }
        public string? Name_en { get; set; }
        public string? Name_kr { get; set; }
        public string? Name_cn { get; set; }
        public string? Icon { get; set; }

        public string DisplayName(string lang) => lang switch
        {
            "en" => Name_en ?? Name_vi ?? "",
            "ko" => Name_kr ?? Name_vi ?? "",
            "zh" => Name_cn ?? Name_vi ?? "",
            _ => Name_vi ?? ""
        };
    }
}