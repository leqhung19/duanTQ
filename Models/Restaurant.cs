namespace DoAn.FRONTEND.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? Description_vi { get; set; }
        public string? Description_en { get; set; }
        public string? Description_kr { get; set; }
        public string? Description_cn { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}